using System.Net;

using Microsoft.Extensions.Logging;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Api.IntegrationTests;

namespace MX.GeoLocation.Api.Client.IntegrationTests;

[Trait("Category", "Integration")]
public class ClientV1LookupTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly GeoLookupApi _geoLookupApi;

    public ClientV1LookupTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetMocks();
        _httpClient = _factory.CreateClient();
        var restClientService = new TestServerRestClientService(_httpClient);
        var options = new GeoLocationApiClientOptions { BaseUrl = "http://localhost" };

        _geoLookupApi = new GeoLookupApi(
            Mock.Of<ILogger<BaseApi<GeoLocationApiClientOptions>>>(),
            Mock.Of<IApiTokenProvider>(),
            restClientService,
            options);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetGeoLocation_CacheHit_ReturnsData()
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
        var result = await _geoLookupApi.GetGeoLocation("8.8.8.8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result?.Data);
        Assert.Equal("Mountain View", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetGeoLocation_InvalidHostname_ReturnsError()
    {
        // Act
        var result = await _geoLookupApi.GetGeoLocation("abcdefg");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetGeoLocations_Batch_ReturnsResults()
    {
        // Arrange
        var dto1 = new GeoLocationDto { Address = "8.8.8.8", TranslatedAddress = "8.8.8.8", CityName = "Mountain View", CountryName = "United States" };
        var dto2 = new GeoLocationDto { Address = "1.1.1.1", TranslatedAddress = "1.1.1.1", CityName = "Sydney", CountryName = "Australia" };

        _factory.MockTableStorage.Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>())).ReturnsAsync(dto1);
        _factory.MockTableStorage.Setup(x => x.GetGeoLocation("1.1.1.1", It.IsAny<CancellationToken>())).ReturnsAsync(dto2);

        // Act
        var result = await _geoLookupApi.GetGeoLocations(["8.8.8.8", "1.1.1.1"]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Result?.Data?.Items);
        Assert.Equal(2, result.Result!.Data!.Items!.Count());
    }

    [Fact]
    public async Task DeleteMetadata_ExistingEntry_ReturnsSuccess()
    {
        // Arrange
        _factory.MockTableStorage
            .Setup(x => x.DeleteGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _geoLookupApi.DeleteMetadata("8.8.8.8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    public Task DisposeAsync()
    {
        _httpClient.Dispose();
        return Task.CompletedTask;
    }
}
