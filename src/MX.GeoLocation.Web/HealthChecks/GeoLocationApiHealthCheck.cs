using Microsoft.Extensions.Diagnostics.HealthChecks;
using MX.GeoLocation.Abstractions.Interfaces;

namespace MX.GeoLocation.Web.HealthChecks;

/// <summary>
/// Health check that verifies connectivity to the GeoLocation API
/// </summary>
public class GeoLocationApiHealthCheck : IHealthCheck
{
    private readonly IApiHealthApi _apiHealthApi;

    public GeoLocationApiHealthCheck(IApiHealthApi apiHealthApi)
    {
        _apiHealthApi = apiHealthApi;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiHealthApi.CheckHealth(cancellationToken);

            return result.IsSuccess ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy($"GeoLocation API returned {result.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to GeoLocation API.", ex);
        }
    }
}
