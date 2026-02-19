using System.Net;

using MaxMind.GeoIP2.Exceptions;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1_1;

using Newtonsoft.Json;

namespace MX.GeoLocation.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class V11GeoLookupTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public V11GeoLookupTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetMocks();
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCityGeoLocation_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedDto = new CityGeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            CountryName = "United States",
            CityName = "Mountain View"
        };

        _factory.MockTableStorage
            .Setup(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/city/8.8.8.8");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CityGeoLocationDto>>(content);

        Assert.NotNull(apiResponse?.Data);
        Assert.Equal("Mountain View", apiResponse.Data!.CityName);

        _factory.MockMaxMind.Verify(x => x.GetCityGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCityGeoLocation_CacheMiss_QueriesMaxMindAndStores()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetCityGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CityGeoLocationDto?)null);

        var maxMindDto = new CityGeoLocationDto
        {
            Address = "1.1.1.1",
            TranslatedAddress = "1.1.1.1",
            CountryName = "Australia",
            CityName = "Sydney"
        };

        _factory.MockMaxMind
            .Setup(x => x.GetCityGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxMindDto);

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/city/1.1.1.1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _factory.MockMaxMind.Verify(x => x.GetCityGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()), Times.Once);
        _factory.MockTableStorage.Verify(x => x.StoreCityGeoLocation(It.IsAny<CityGeoLocationDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedDto = new InsightsGeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            CountryName = "United States",
            CityName = "Mountain View",
            Anonymizer = new AnonymizerDto()
        };

        _factory.MockTableStorage
            .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/insights/8.8.8.8");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _factory.MockMaxMind.Verify(x => x.GetInsightsGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_CacheMiss_QueriesMaxMindAndStores()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetInsightsGeoLocation("1.1.1.1", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InsightsGeoLocationDto?)null);

        var maxMindDto = new InsightsGeoLocationDto
        {
            Address = "1.1.1.1",
            TranslatedAddress = "1.1.1.1",
            CountryName = "Australia",
            CityName = "Sydney",
            Anonymizer = new AnonymizerDto()
        };

        _factory.MockMaxMind
            .Setup(x => x.GetInsightsGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxMindDto);

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/insights/1.1.1.1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _factory.MockMaxMind.Verify(x => x.GetInsightsGeoLocation("1.1.1.1", It.IsAny<CancellationToken>()), Times.Once);
        _factory.MockTableStorage.Verify(x => x.StoreInsightsGeoLocation(It.IsAny<InsightsGeoLocationDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("abcdefg")]
    [InlineData("a.b.c.d")]
    public async Task GetCityGeoLocation_InvalidHostname_ReturnsBadRequest(string invalidHostname)
    {
        var response = await _client.GetAsync($"/v1.1/lookup/city/{invalidHostname}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    public async Task GetCityGeoLocation_LocalAddress_ReturnsBadRequest(string localAddress)
    {
        var response = await _client.GetAsync($"/v1.1/lookup/city/{localAddress}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("abcdefg")]
    [InlineData("a.b.c.d")]
    public async Task GetInsightsGeoLocation_InvalidHostname_ReturnsBadRequest(string invalidHostname)
    {
        var response = await _client.GetAsync($"/v1.1/lookup/insights/{invalidHostname}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    public async Task GetInsightsGeoLocation_LocalAddress_ReturnsBadRequest(string localAddress)
    {
        var response = await _client.GetAsync($"/v1.1/lookup/insights/{localAddress}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCityGeoLocation_AddressNotFound_ReturnsNotFound()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetCityGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CityGeoLocationDto?)null);

        _factory.MockMaxMind
            .Setup(x => x.GetCityGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AddressNotFoundException("Address not found"));

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/city/198.51.100.1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCityGeoLocation_GeoIP2Exception_ReturnsBadRequest()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetCityGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CityGeoLocationDto?)null);

        _factory.MockMaxMind
            .Setup(x => x.GetCityGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GeoIP2Exception("GeoIP2 service error"));

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/city/203.0.113.1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_AddressNotFound_ReturnsNotFound()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetInsightsGeoLocation("198.51.100.1", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InsightsGeoLocationDto?)null);

        _factory.MockMaxMind
            .Setup(x => x.GetInsightsGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AddressNotFoundException("Address not found"));

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/insights/198.51.100.1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_GeoIP2Exception_ReturnsBadRequest()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.GetInsightsGeoLocation("203.0.113.1", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InsightsGeoLocationDto?)null);

        _factory.MockMaxMind
            .Setup(x => x.GetInsightsGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GeoIP2Exception("GeoIP2 service error"));

        // Act
        var response = await _client.GetAsync("/v1.1/lookup/insights/203.0.113.1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
