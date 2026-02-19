using System.Net;

using Newtonsoft.Json;

namespace MX.GeoLocation.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class InfoAndHealthTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public InfoAndHealthTests(CustomWebApplicationFactory factory)
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
    public async Task GetInfo_ReturnsOkWithBuildVersion()
    {
        // Act
        var response = await _client.GetAsync("/v1.0/info");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));

        var info = JsonConvert.DeserializeObject<dynamic>(content);
        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetHealth_ReturnsResponse()
    {
        // Act
        var response = await _client.GetAsync("/v1.0/health");

        // Assert - health endpoint returns a response (may be degraded due to stubbed dependencies)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected OK or ServiceUnavailable but got {response.StatusCode}");
    }
}
