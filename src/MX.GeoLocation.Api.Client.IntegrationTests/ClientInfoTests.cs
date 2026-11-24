using System.Net;

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

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

    [Theory]
    [InlineData(HealthStatus.Healthy, true)]
    [InlineData(HealthStatus.Unhealthy, false)]
    public async Task CheckHealth_ReturnsApiReadinessStatus(HealthStatus healthStatus, bool expectedSuccess)
    {
        var healthService = new Mock<HealthCheckService>();
        var entry = new HealthReportEntry(healthStatus, null, TimeSpan.Zero, null, null);
        var report = new HealthReport(new Dictionary<string, HealthReportEntry> { ["test"] = entry }, TimeSpan.Zero);
        healthService.Setup(service => service.CheckHealthAsync(
                It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<HealthCheckService>();
            services.AddSingleton(healthService.Object);
        }));
        using var client = factory.CreateClient();
        var healthApi = new ApiHealthApi(
            Mock.Of<ILogger<BaseApi<GeoLocationApiClientOptions>>>(),
            Mock.Of<IApiTokenProvider>(),
            new TestServerRestClientService(client),
            new GeoLocationApiClientOptions { BaseUrl = "http://localhost" });

        var result = await healthApi.CheckHealth();

        Assert.Equal(expectedSuccess, result.IsSuccess);
        healthService.Verify(service => service.CheckHealthAsync(
            It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
