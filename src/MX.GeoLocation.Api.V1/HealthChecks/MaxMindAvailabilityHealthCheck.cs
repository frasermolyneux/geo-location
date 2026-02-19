using System.Diagnostics;

using MaxMind.GeoIP2;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MX.GeoLocation.LookupWebApi.HealthChecks;

/// <summary>
/// Health check that verifies MaxMind GeoIP2 web service is reachable
/// by performing a lightweight city lookup and reporting availability telemetry.
/// Results are cached for a short TTL to avoid consuming paid API queries on every poll.
/// </summary>
public class MaxMindAvailabilityHealthCheck : IHealthCheck
{
    private const string ProbeAddress = "1.1.1.1";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly WebServiceClient _webServiceClient;
    private readonly TelemetryClient _telemetryClient;

    private static HealthCheckResult? _cachedResult;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly object _cacheLock = new();

    public MaxMindAvailabilityHealthCheck(WebServiceClient webServiceClient, TelemetryClient telemetryClient)
    {
        _webServiceClient = webServiceClient;
        _telemetryClient = telemetryClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        lock (_cacheLock)
        {
            if (_cachedResult.HasValue && DateTime.UtcNow < _cacheExpiry)
                return _cachedResult.Value;
        }

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

            var healthResult = HealthCheckResult.Healthy(availability.Message);

            lock (_cacheLock)
            {
                _cachedResult = healthResult;
                _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
            }

            return healthResult;
        }
        catch (Exception ex)
        {
            availability.Success = false;
            availability.Message = ex.Message;

            var healthResult = HealthCheckResult.Unhealthy($"MaxMind API call failed: {ex.Message}");

            lock (_cacheLock)
            {
                _cachedResult = healthResult;
                _cacheExpiry = DateTime.UtcNow.Add(TimeSpan.FromMinutes(1));
            }

            return healthResult;
        }
        finally
        {
            stopwatch.Stop();
            availability.Duration = stopwatch.Elapsed;
            _telemetryClient.TrackAvailability(availability);
        }
    }
}
