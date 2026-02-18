using System.Net;

using Microsoft.Extensions.Logging;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Api.IntegrationTests;

namespace MX.GeoLocation.Api.Client.IntegrationTests;

public class ClientV11LookupTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly GeoLookupApiV1_1 _geoLookupApi;

    public ClientV11LookupTests()
    {
        _factory = new CustomWebApplicationFactory();
        var httpClient = _factory.CreateClient();
        var restClientService = new TestServerRestClientService(httpClient);
        var options = new GeoLocationApiClientOptions { BaseUrl = "http://localhost" };

        _geoLookupApi = new GeoLookupApiV1_1(
            Mock.Of<ILogger<BaseApi<GeoLocationApiClientOptions>>>(),
            Mock.Of<IApiTokenProvider>(),
            restClientService,
            options);
    }

    [Fact]
    public async Task GetCityGeoLocation_CacheHit_ReturnsData()
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
        var result = await _geoLookupApi.GetCityGeoLocation("8.8.8.8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result?.Data);
        Assert.Equal("Mountain View", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_CacheHit_ReturnsData()
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
        var result = await _geoLookupApi.GetInsightsGeoLocation("8.8.8.8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result?.Data);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
