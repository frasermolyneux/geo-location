using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MX.GeoLocation.Api.Client.Testing;
using MX.GeoLocation.Api.Client.V1;

namespace MX.GeoLocation.Web.IntegrationTests;

/// <summary>
/// Hosts the Web app on a real Kestrel port so Playwright can connect to it.
/// Replaces IGeoLocationApiClient with <see cref="FakeGeoLocationApiClient"/> from the testing package.
/// Uses the Web project's content root for views and static files.
/// </summary>
public class WebAppFactory : IAsyncDisposable
{
    private WebApplication? _app;

    public string BaseUrl { get; private set; } = string.Empty;
    public FakeGeoLocationApiClient FakeApiClient { get; } = new();

    public WebAppFactory()
    {
        ConfigureFakeApiClient();
    }

    public async Task StartAsync()
    {
        // Resolve the Web project's source directory for content root (views, wwwroot)
        var webProjectDir = FindWebProjectDirectory();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development",
            ContentRootPath = webProjectDir,
            WebRootPath = Path.Combine(webProjectDir, "wwwroot")
        });

        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GeoLocationApi:BaseUrl"] = "http://localhost",
            ["GeoLocationApi:ApiKey"] = "test-key",
            ["GeoLocationApi:ApplicationAudience"] = "api://test",
            ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost",
        });

        // Replicate the real app's service registrations
        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
        {
            EnableAdaptiveSampling = false,
        });
        builder.Services.AddControllersWithViews()
            .AddApplicationPart(typeof(Program).Assembly);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession();
        builder.Services.AddHealthChecks();

        // Replace the API client with the testing package fake (dogfooding)
        builder.Services.AddFakeGeoLocationApiClient(_ =>
        {
            // FakeApiClient is already configured in the constructor
        });
        // Re-register with the pre-configured instance
        builder.Services.RemoveAll<IGeoLocationApiClient>();
        builder.Services.AddSingleton<IGeoLocationApiClient>(FakeApiClient);

        // Stub TelemetryConfiguration
        builder.Services.RemoveAll<TelemetryConfiguration>();
        builder.Services.AddSingleton(new TelemetryConfiguration
        {
            ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost"
        });

        // Remove ServiceProfiler hosted services to avoid noisy "Instrumentation Key is empty" errors
        var profilerDescriptors = builder.Services
            .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                        d.ImplementationType?.FullName?.Contains("Profiler") == true)
            .ToList();
        foreach (var descriptor in profilerDescriptors)
        {
            builder.Services.Remove(descriptor);
        }

        _app = builder.Build();

        _app.UseSession();
        _app.UseStaticFiles();
        _app.UseRouting();
        _app.UseAuthorization();
        _app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        _app.MapHealthChecks("/api/health").AllowAnonymous();

        await _app.StartAsync();

        BaseUrl = _app.Urls.First();
    }

    private static string FindWebProjectDirectory()
    {
        // Walk up from the test assembly location to find the Web project directory
        var assemblyDir = Path.GetDirectoryName(typeof(WebAppFactory).Assembly.Location)!;
        var dir = new DirectoryInfo(assemblyDir);

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "MX.GeoLocation.Web");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "MX.GeoLocation.Web.csproj")))
                return candidate;

            // Also check src subdirectory
            candidate = Path.Combine(dir.FullName, "src", "MX.GeoLocation.Web");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "MX.GeoLocation.Web.csproj")))
                return candidate;

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find MX.GeoLocation.Web project directory");
    }

    private void ConfigureFakeApiClient()
    {
        FakeApiClient.V1Lookup
            .AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation(
                address: "8.8.8.8",
                cityName: "Mountain View",
                countryName: "United States",
                countryCode: "US",
                continentName: "North America",
                latitude: 37.386,
                longitude: -122.0838))
            .AddResponse("1.1.1.1", GeoLocationDtoFactory.CreateGeoLocation(
                address: "1.1.1.1",
                cityName: "Los Angeles",
                countryName: "United States",
                countryCode: "US",
                continentName: "North America",
                latitude: 34.0522,
                longitude: -118.2437));
    }

    public async ValueTask DisposeAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
