using System.Diagnostics;

using MaxMind.GeoIP2;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MX.GeoLocation.LookupWebApi.HealthChecks;

/// <summary>
/// Health check that verifies MaxMind GeoIP2 web service is reachable
/// by performing a lightweight city lookup and reporting availability telemetry.
/// </summary>
public class MaxMindAvailabilityHealthCheck : IHealthCheck
{
    // Well-known public IP (Cloudflare DNS) used for the probe
    private const string ProbeAddress = "1.1.1.1";

    private readonly IConfiguration _configuration;
    private readonly TelemetryClient _telemetryClient;

    public MaxMindAvailabilityHealthCheck(IConfiguration configuration, TelemetryClient telemetryClient)
    {
        _configuration = configuration;
        _telemetryClient = telemetryClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var availability = new AvailabilityTelemetry
        {
            Name = "MaxMind GeoIP2 API",
            RunLocation = Environment.MachineName
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var userIdString = _configuration["maxmind_userid"];
            if (!int.TryParse(userIdString, out var userId))
            {
                availability.Success = false;
                availability.Message = "MaxMind user ID is not configured or invalid.";
                return HealthCheckResult.Unhealthy(availability.Message);
            }

            var apiKey = _configuration["maxmind_apikey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                availability.Success = false;
                availability.Message = "MaxMind API key is not configured.";
                return HealthCheckResult.Unhealthy(availability.Message);
            }

            using var client = new WebServiceClient(userId, apiKey);
            var result = await client.CityAsync(ProbeAddress);

            availability.Success = true;
            availability.Message = $"Lookup succeeded for {ProbeAddress} — {result.Country?.Name ?? "unknown"}";

            return HealthCheckResult.Healthy(availability.Message);
        }
        catch (Exception ex)
        {
            availability.Success = false;
            availability.Message = ex.Message;

            return HealthCheckResult.Unhealthy($"MaxMind API call failed: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            availability.Duration = stopwatch.Elapsed;
            _telemetryClient.TrackAvailability(availability);
        }
    }
}
