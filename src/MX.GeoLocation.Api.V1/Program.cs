using Azure.Identity;
using Azure.Data.Tables;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Identity.Web;

using MX.GeoLocation.LookupWebApi;
using MX.GeoLocation.LookupWebApi.OpenApi;
using MX.GeoLocation.LookupWebApi.Repositories;

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
    // Format the version as "'v'major" (e.g. v1)
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Configure OpenAPI
builder.Services.AddOpenApi("v1", options =>
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

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
