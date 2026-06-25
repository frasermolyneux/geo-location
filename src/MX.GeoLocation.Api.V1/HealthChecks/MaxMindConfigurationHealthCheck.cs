using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MX.GeoLocation.LookupWebApi.HealthChecks;

/// <summary>
/// Health check that verifies MaxMind GeoIP2 configuration is present and valid
/// </summary>
public class MaxMindConfigurationHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public MaxMindConfigurationHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var userId = _configuration["maxmind_userid"];
        var apiKey = _configuration["maxmind_apikey"];

        return string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out _)
            ? Task.FromResult(HealthCheckResult.Unhealthy("MaxMind user ID is not configured or invalid."))
            : string.IsNullOrWhiteSpace(apiKey)
            ? Task.FromResult(HealthCheckResult.Unhealthy("MaxMind API key is not configured."))
            : Task.FromResult(HealthCheckResult.Healthy());
    }
}
