using System.Net;
using System.Text;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;

using Newtonsoft.Json;

namespace MX.GeoLocation.Api.IntegrationTests;

public class V1BatchLookupTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public V1BatchLookupTests()
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
    public async Task GetGeoLocations_ValidBatch_ReturnsResults()
    {
        // Arrange
        var dto1 = new GeoLocationDto { Address = "8.8.8.8", TranslatedAddress = "8.8.8.8", CityName = "Mountain View", CountryName = "United States" };
        var dto2 = new GeoLocationDto { Address = "1.1.1.1", TranslatedAddress = "1.1.1.1", CityName = "Sydney", CountryName = "Australia" };

        _factory.MockTableStorage.Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>())).ReturnsAsync(dto1);
        _factory.MockTableStorage.Setup(x => x.GetGeoLocation("1.1.1.1", It.IsAny<CancellationToken>())).ReturnsAsync(dto2);

        var hostnames = JsonConvert.SerializeObject(new[] { "8.8.8.8", "1.1.1.1" });
        var content = new StringContent(hostnames, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1.0/lookup", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CollectionModel<GeoLocationDto>>>(responseContent);

        Assert.NotNull(apiResponse?.Data?.Items);
        Assert.Equal(2, apiResponse.Data!.Items!.Count());
    }

    [Fact]
    public async Task GetGeoLocations_MixedValidAndLocalhost_ReturnsPartialWithErrors()
    {
        // Arrange
        var dto = new GeoLocationDto { Address = "8.8.8.8", TranslatedAddress = "8.8.8.8", CityName = "Mountain View", CountryName = "United States" };
        _factory.MockTableStorage.Setup(x => x.GetGeoLocation("8.8.8.8", It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var hostnames = JsonConvert.SerializeObject(new[] { "8.8.8.8", "localhost" });
        var content = new StringContent(hostnames, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1.0/lookup", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CollectionModel<GeoLocationDto>>>(responseContent);

        Assert.NotNull(apiResponse?.Data?.Items);
        Assert.Single(apiResponse.Data!.Items!);
        Assert.NotNull(apiResponse.Errors);
        Assert.NotEmpty(apiResponse.Errors!);
    }

    [Fact]
    public async Task GetGeoLocations_InvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("not valid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1.0/lookup", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
