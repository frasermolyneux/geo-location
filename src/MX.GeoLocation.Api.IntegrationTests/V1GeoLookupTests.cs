using System.Net;
using System.Net.Http.Json;

using MaxMind.GeoIP2.Exceptions;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;

using Newtonsoft.Json;

namespace MX.GeoLocation.Api.IntegrationTests;

public class V1GeoLookupTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public V1GeoLookupTests()
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

        _factory.MockTableStorage
            .Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        // Act
        var response = await _client.GetAsync("/v1.0/lookup/8.8.8.8");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<GeoLocationDto>>(content);

        Assert.NotNull(apiResponse?.Data);
        Assert.Equal("Mountain View", apiResponse.Data!.CityName);
        Assert.Equal("United States", apiResponse.Data.CountryName);

        _factory.MockMaxMind.Verify(x => x.GetGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetGeoLocation_CacheMiss_QueriesMaxMindAndStores()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        var maxMindDto = new GeoLocationDto
        {
            Address = "1.1.1.1",
            TranslatedAddress = "1.1.1.1",
            CountryName = "Australia",
            CityName = "Sydney"
        };

        _factory.MockMaxMind
            .Setup(x => x.GetGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxMindDto);

        // Act
        var response = await _client.GetAsync("/v1.0/lookup/1.1.1.1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<GeoLocationDto>>(content);

        Assert.NotNull(apiResponse?.Data);
        Assert.Equal("1.1.1.1", apiResponse.Data!.Address);

        _factory.MockMaxMind.Verify(x => x.GetGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()), Times.Once);
        _factory.MockTableStorage.Verify(x => x.StoreGeoLocation(It.IsAny<GeoLocationDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("abcdefg")]
    [InlineData("a.b.c.d")]
    public async Task GetGeoLocation_InvalidHostname_ReturnsBadRequest(string invalidHostname)
    {
        // Act
        var response = await _client.GetAsync($"/v1.0/lookup/{invalidHostname}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    public async Task GetGeoLocation_LocalAddress_ReturnsBadRequest(string localAddress)
    {
        // Act
        var response = await _client.GetAsync($"/v1.0/lookup/{localAddress}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<GeoLocationDto>>(content);

        Assert.NotNull(apiResponse?.Errors);
        Assert.NotEmpty(apiResponse.Errors!);
    }

    [Fact]
    public async Task GetGeoLocation_AddressNotFound_ReturnsNotFound()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        _factory.MockMaxMind
            .Setup(x => x.GetGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AddressNotFoundException("Address not found"));

        // Act
        var response = await _client.GetAsync("/v1.0/lookup/198.51.100.1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGeoLocation_GeoIP2Exception_ReturnsBadRequest()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoLocationDto?)null);

        _factory.MockMaxMind
            .Setup(x => x.GetGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GeoIP2Exception("GeoIP2 service error"));

        // Act
        var response = await _client.GetAsync("/v1.0/lookup/203.0.113.1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
