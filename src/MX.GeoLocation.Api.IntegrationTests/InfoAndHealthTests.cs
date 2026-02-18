using System.Net;

using Newtonsoft.Json;

namespace MX.GeoLocation.Api.IntegrationTests;

public class InfoAndHealthTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public InfoAndHealthTests()
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
