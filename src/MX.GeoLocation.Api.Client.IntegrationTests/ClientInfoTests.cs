using System.Net;

using Microsoft.Extensions.Logging;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Api.IntegrationTests;

namespace MX.GeoLocation.Api.Client.IntegrationTests;

[Trait("Category", "Integration")]
public class ClientInfoTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly ApiInfoApi _apiInfoApi;

    public ClientInfoTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetMocks();
        _httpClient = _factory.CreateClient();
        var restClientService = new TestServerRestClientService(_httpClient);
        var options = new GeoLocationApiClientOptions { BaseUrl = "http://localhost" };

        _apiInfoApi = new ApiInfoApi(
            Mock.Of<ILogger<BaseApi<GeoLocationApiClientOptions>>>(),
            Mock.Of<IApiTokenProvider>(),
            restClientService,
            options);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _httpClient.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetApiInfo_ReturnsInfo()
    {
        // Act
        var result = await _apiInfoApi.GetApiInfo();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
