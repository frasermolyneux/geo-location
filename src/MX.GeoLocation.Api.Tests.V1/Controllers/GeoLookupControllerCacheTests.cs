using System.Net;

using MaxMind.GeoIP2.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Controllers.V1;
using MX.GeoLocation.LookupWebApi.Repositories;

namespace MX.GeoLocation.LookupWebApi.Tests.Controllers;

public class GeoLookupControllerCacheTests : IDisposable
{
    private readonly Mock<ITableStorageGeoLocationRepository> _mockTableStorage;
    private readonly Mock<IMaxMindGeoLocationRepository> _mockMaxMind;
    private readonly GeoLookupController _controller;

    public GeoLookupControllerCacheTests()
    {
        _mockTableStorage = new Mock<ITableStorageGeoLocationRepository>();
        _mockMaxMind = new Mock<IMaxMindGeoLocationRepository>();

        _controller = new GeoLookupController(
            Mock.Of<ILogger<GeoLookupController>>(),
            _mockTableStorage.Object,
            _mockMaxMind.Object);
    }

    [Fact]
    public async Task GetGeoLocation_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedDto = new GeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            CountryName = "United States",
            CityName = "Mountain View"
        };

        _mockTableStorage
            .Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _controller.GetGeoLocation("8.8.8.8", CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(200, objectResult.StatusCode);

        _mockMaxMind.Verify(x => x.GetGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetGeoLocation_CacheMiss_QueriesMaxMindAndStores()
    {
        // Arrange
        _mockTableStorage
            .Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        var maxMindDto = new GeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            CountryName = "United States",
            CityName = "Mountain View"
        };

        _mockMaxMind
            .Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxMindDto);

        // Act
        var result = await _controller.GetGeoLocation("8.8.8.8", CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(200, objectResult.StatusCode);

        _mockMaxMind.Verify(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()), Times.Once);
        _mockTableStorage.Verify(x => x.StoreGeoLocation(It.IsAny<GeoLocationDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGeoLocation_AddressNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockTableStorage
            .Setup(x => x.GetGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        _mockMaxMind
            .Setup(x => x.GetGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AddressNotFoundException("Address not found"));

        // Act
        var result = await _controller.GetGeoLocation("198.51.100.1", CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetGeoLocation_GeoIP2Exception_ReturnsBadRequest()
    {
        // Arrange
        _mockTableStorage
            .Setup(x => x.GetGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        _mockMaxMind
            .Setup(x => x.GetGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GeoIP2Exception("Service error"));

        // Act
        var result = await _controller.GetGeoLocation("203.0.113.1", CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetGeoLocation_UnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        _mockTableStorage
            .Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        _mockMaxMind
            .Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected"));

        // Act
        var result = await _controller.GetGeoLocation("8.8.8.8", CancellationToken.None);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(500, objectResult.StatusCode);
    }

    public void Dispose()
    {
        _controller?.Dispose();
    }
}
