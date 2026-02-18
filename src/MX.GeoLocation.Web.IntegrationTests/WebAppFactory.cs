using System.Net;
using System.Reflection;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Api.Client.V1;

namespace MX.GeoLocation.Web.IntegrationTests;

/// <summary>
/// Hosts the Web app on a real Kestrel port so Playwright can connect to it.
/// Replaces IGeoLocationApiClient with a Moq instance returning canned data.
/// Uses the Web project's content root for views and static files.
/// </summary>
public class WebAppFactory : IAsyncDisposable
{
    private WebApplication? _app;

    public string BaseUrl { get; private set; } = string.Empty;
    public Mock<IGeoLocationApiClient> MockApiClient { get; } = new();

    public WebAppFactory()
    {
        ConfigureMockApiClient();
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

        // Replace the API client with our mock (don't call AddGeoLocationApiClient)
        builder.Services.AddSingleton(MockApiClient.Object);

        // Stub TelemetryConfiguration
        builder.Services.RemoveAll<TelemetryConfiguration>();
        builder.Services.AddSingleton(new TelemetryConfiguration
        {
            ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost"
        });

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

    private void ConfigureMockApiClient()
    {
        var mockGeoLookup = new Mock<IVersionedGeoLookupApi>();
        var mockV1 = new Mock<IGeoLookupApi>();

        mockV1.Setup(x => x.GetGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string address, CancellationToken _) =>
            {
                var dto = GetCannedResponse(address);
                return new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(dto));
            });

        mockV1.Setup(x => x.GetGeoLocations(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string> addresses, CancellationToken _) =>
            {
                var items = addresses.Select(GetCannedResponse).ToList();
                var data = new CollectionModel<GeoLocationDto> { Items = items };
                return new ApiResult<CollectionModel<GeoLocationDto>>(HttpStatusCode.OK,
                    new ApiResponse<CollectionModel<GeoLocationDto>>(data));
            });

        mockV1.Setup(x => x.DeleteMetadata(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(HttpStatusCode.OK, new ApiResponse()));

        mockGeoLookup.Setup(x => x.V1).Returns(mockV1.Object);
        MockApiClient.Setup(x => x.GeoLookup).Returns(mockGeoLookup.Object);
    }

    public static GeoLocationDto GetCannedResponse(string address)
    {
        return address switch
        {
            "8.8.8.8" => new GeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CityName = "Mountain View",
                CountryName = "United States",
                CountryCode = "US",
                ContinentName = "North America",
                Latitude = 37.386,
                Longitude = -122.0838
            },
            "1.1.1.1" => new GeoLocationDto
            {
                Address = "1.1.1.1",
                TranslatedAddress = "1.1.1.1",
                CityName = "Los Angeles",
                CountryName = "United States",
                CountryCode = "US",
                ContinentName = "North America",
                Latitude = 34.0522,
                Longitude = -118.2437
            },
            _ => new GeoLocationDto
            {
                Address = address,
                TranslatedAddress = address,
                CityName = "Test City",
                CountryName = "Test Country",
                CountryCode = "TC",
                ContinentName = "Test Continent",
                Latitude = 0,
                Longitude = 0
            }
        };
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
