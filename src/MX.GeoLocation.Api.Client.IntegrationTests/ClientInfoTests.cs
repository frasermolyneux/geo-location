using System.Net;

using Microsoft.Extensions.Logging;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Api.IntegrationTests;

namespace MX.GeoLocation.Api.Client.IntegrationTests;

public class ClientInfoTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly ApiInfoApi _apiInfoApi;

    public ClientInfoTests()
    {
        _factory = new CustomWebApplicationFactory();
        var httpClient = _factory.CreateClient();
        var restClientService = new TestServerRestClientService(httpClient);
        var options = new GeoLocationApiClientOptions { BaseUrl = "http://localhost" };

        _apiInfoApi = new ApiInfoApi(
            Mock.Of<ILogger<BaseApi<GeoLocationApiClientOptions>>>(),
            Mock.Of<IApiTokenProvider>(),
            restClientService,
            options);
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

    public void Dispose()
    {
        _factory.Dispose();
    }
}
