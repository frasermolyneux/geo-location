using System.Net;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;

using Newtonsoft.Json;

namespace MX.GeoLocation.Api.IntegrationTests;

public class V1DeleteTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public V1DeleteTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task DeleteMetadata_ExistingEntry_ReturnsSuccess()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.DeleteGeoLocation("8.8.8.8"))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync("/v1.0/lookup/8.8.8.8");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMetadata_NonExistingEntry_ReturnsNotFound()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.DeleteGeoLocation("192.0.2.1"))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync("/v1.0/lookup/192.0.2.1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("abcdefg")]
    [InlineData("a.b.c.d")]
    public async Task DeleteMetadata_InvalidHostname_ReturnsBadRequest(string invalidHostname)
    {
        // Act
        var response = await _client.DeleteAsync($"/v1.0/lookup/{invalidHostname}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    public async Task DeleteMetadata_LocalAddress_ReturnsBadRequest(string localAddress)
    {
        // Act
        var response = await _client.DeleteAsync($"/v1.0/lookup/{localAddress}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
