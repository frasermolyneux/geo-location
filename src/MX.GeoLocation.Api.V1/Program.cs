using Azure.Identity;
using Azure.Data.Tables;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Identity.Web;

using MX.GeoLocation.LookupWebApi;
using MX.GeoLocation.LookupWebApi.OpenApi;
using MX.GeoLocation.LookupWebApi.Repositories;

using MX.GeoLocation.LookupWebApi.HealthChecks;
using Newtonsoft.Json.Converters;
using Asp.Versioning;
using Microsoft.ApplicationInsights.WindowsServer.Channel.Implementation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    // Configure URL path versioning
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    // Format the version as "'v'major.minor" (e.g. v1.0, v1.1)
    options.GroupNameFormat = "'v'VV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure OpenAPI
builder.Services.AddOpenApi("v1.0", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer<StripVersionPrefixTransformer>();
});

builder.Services.AddOpenApi("v1.1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer<StripVersionPrefixTransformer>();
});

builder.Services.AddSingleton<TableServiceClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var tableEndpoint = configuration["Storage:TableEndpoint"];
    if (string.IsNullOrWhiteSpace(tableEndpoint))
        throw new InvalidOperationException("Storage:TableEndpoint is not configured.");
    return new TableServiceClient(new Uri(tableEndpoint), new DefaultAzureCredential());
});

builder.Services.AddSingleton<ITableStorageGeoLocationRepository, TableStorageGeoLocationRepository>();
builder.Services.AddSingleton<IMaxMindGeoLocationRepository, MaxMindGeoLocationRepository>();

builder.Services.AddHealthChecks()
    .AddAzureTable(
        name: "azure-table-storage",
        tags: ["dependency"])
    .AddCheck<MaxMindConfigurationHealthCheck>(
        name: "maxmind-configuration",
        tags: ["dependency"])
    .AddCheck<MaxMindAvailabilityHealthCheck>(
        name: "maxmind-availability",
        tags: ["dependency"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests
public partial class Program { }
