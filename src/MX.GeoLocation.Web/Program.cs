using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.Channel.Implementation;
using Microsoft.AspNetCore.HttpOverrides;
using MX.Api.Client.Extensions;
using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Web;
using MX.GeoLocation.Web.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();

//https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling-classic-api#configure-sampling-settings
builder.Services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
{
    var telemetryProcessorChainBuilder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
    telemetryProcessorChainBuilder.UseAdaptiveSampling(
        settings: new SamplingPercentageEstimatorSettings
        {
            InitialSamplingPercentage = 5,
            MinSamplingPercentage = 5,
            MaxSamplingPercentage = 60
        },
        callback: null,
        excludedTypes: "Exception");
    telemetryProcessorChainBuilder.Build();
});
builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    EnableAdaptiveSampling = false,
});

builder.Services.AddServiceProfiler();

builder.Services.AddControllersWithViews();

builder.Services.AddGeoLocationApiClient(options => options
    .WithBaseUrl(builder.Configuration["GeoLocationApi:BaseUrl"] ?? throw new ArgumentNullException("GeoLocationApi:BaseUrl"))
    .WithApiKeyAuthentication(builder.Configuration["GeoLocationApi:ApiKey"] ?? throw new ArgumentNullException("GeoLocationApi:ApiKey"))
    .WithEntraIdAuthentication(builder.Configuration["GeoLocationApi:ApplicationAudience"] ?? throw new ArgumentNullException("GeoLocationApi:ApplicationAudience")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();

// Configure ForwardedHeaders to securely process X-Forwarded-For and X-Forwarded-Proto via middleware
// instead of manually parsing these headers in controllers.
// NOTE: In production, configure options.KnownProxies or options.KnownNetworks to restrict
// which reverse proxy IPs are trusted (e.g. your Azure Front Door or load balancer IPs).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddHealthChecks()
    .AddCheck<GeoLocationApiHealthCheck>(
        name: "geolocation-api",
        tags: ["dependency"]);

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/api/health").AllowAnonymous();

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests
public partial class Program { }
