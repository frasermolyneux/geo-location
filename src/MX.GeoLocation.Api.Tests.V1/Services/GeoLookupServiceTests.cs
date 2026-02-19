using System.Net;

using MaxMind.GeoIP2.Exceptions;

using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Web.Extensions;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.Api.Tests.V1.Services;

[Trait("Category", "Unit")]
public class GeoLookupServiceTests
{
    private readonly Mock<IHostnameResolver> _mockHostnameResolver;
    private readonly GeoLookupService _service;

    public GeoLookupServiceTests()
    {
        _mockHostnameResolver = new Mock<IHostnameResolver>();
        _service = new GeoLookupService(
            Mock.Of<ILogger<GeoLookupService>>(),
            _mockHostnameResolver.Object);
    }

    [Fact]
    public async Task ExecuteLookup_ValidAddress_CallsLookupFunc()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "8.8.8.8"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("8.8.8.8")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("8.8.8.8")).Returns(false);

        var dto = new GeoLocationDto { Address = "8.8.8.8", TranslatedAddress = "8.8.8.8" };
        var expectedResult = new ApiResponse<GeoLocationDto>(dto).ToApiResult();

        // Act
        var result = await _service.ExecuteLookup("8.8.8.8", CancellationToken.None, _ =>
            Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_InvalidHostname_ReturnsBadRequest()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("invalid-host", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (string?)null));

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("invalid-host", CancellationToken.None,
            _ => throw new InvalidOperationException("Should not be called"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_LocalAddress_ReturnsBadRequest()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("localhost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "127.0.0.1"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("localhost")).Returns(true);

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("localhost", CancellationToken.None,
            _ => throw new InvalidOperationException("Should not be called"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_PrivateAddress_ReturnsBadRequest()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("192.168.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "192.168.1.1"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("192.168.1.1")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("192.168.1.1")).Returns(true);

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("192.168.1.1", CancellationToken.None,
            _ => throw new InvalidOperationException("Should not be called"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_AddressNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "8.8.8.8"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("8.8.8.8")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("8.8.8.8")).Returns(false);

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("8.8.8.8", CancellationToken.None,
            _ => throw new AddressNotFoundException("Not found"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_GeoIP2Exception_ReturnsBadRequest()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "8.8.8.8"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("8.8.8.8")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("8.8.8.8")).Returns(false);

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("8.8.8.8", CancellationToken.None,
            _ => throw new GeoIP2Exception("Service error"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_UnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "8.8.8.8"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("8.8.8.8")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("8.8.8.8")).Returns(false);

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("8.8.8.8", CancellationToken.None,
            _ => throw new InvalidOperationException("Unexpected"));

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_PassesResolvedAddressToLookupFunc()
    {
        // Arrange
        _mockHostnameResolver.Setup(x => x.ResolveHostname("google.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "142.250.187.195"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("google.com")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("142.250.187.195")).Returns(false);

        string? capturedAddress = null;
        var dto = new GeoLocationDto { Address = "google.com", TranslatedAddress = "142.250.187.195" };

        // Act
        await _service.ExecuteLookup("google.com", CancellationToken.None, address =>
        {
            capturedAddress = address;
            return Task.FromResult(new ApiResponse<GeoLocationDto>(dto).ToApiResult());
        });

        // Assert
        Assert.Equal("142.250.187.195", capturedAddress);
    }

    [Fact]
    public async Task ExecuteLookup_CancelledToken_ReturnsInternalServerError()
    {
        // Arrange - OperationCanceledException from hostname resolution is caught by the generic catch block
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHostnameResolver.Setup(x => x.ResolveHostname(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.ExecuteLookup<GeoLocationDto>("8.8.8.8", cts.Token,
            _ => throw new InvalidOperationException("Should not be called"));

        // Assert - service catches all exceptions and returns appropriate status
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteLookup_CancelledTokenInLookupFunc_ReturnsInternalServerError()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "8.8.8.8"));
        _mockHostnameResolver.Setup(x => x.IsLocalAddress("8.8.8.8")).Returns(false);
        _mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("8.8.8.8")).Returns(false);

        cts.Cancel();

        // OperationCanceledException is caught by the generic catch block
        var result = await _service.ExecuteLookup<GeoLocationDto>("8.8.8.8", cts.Token,
            _ => throw new OperationCanceledException());

        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
