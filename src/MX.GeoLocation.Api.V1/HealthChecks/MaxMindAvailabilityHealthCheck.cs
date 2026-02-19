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
    private const string ProbeAddress = "1.1.1.1";

    private readonly WebServiceClient _webServiceClient;
    private readonly TelemetryClient _telemetryClient;

    public MaxMindAvailabilityHealthCheck(WebServiceClient webServiceClient, TelemetryClient telemetryClient)
    {
        _webServiceClient = webServiceClient;
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
            var result = await _webServiceClient.CityAsync(ProbeAddress);

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
