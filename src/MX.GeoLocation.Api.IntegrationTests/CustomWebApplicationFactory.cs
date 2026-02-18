using System.Security.Claims;
using System.Text.Encodings.Web;

using Azure.Data.Tables;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MX.GeoLocation.LookupWebApi.Repositories;

namespace MX.GeoLocation.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IMaxMindGeoLocationRepository> MockMaxMind { get; } = new();
    public Mock<ITableStorageGeoLocationRepository> MockTableStorage { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Provide required configuration values to prevent startup errors
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:TableEndpoint"] = "https://fake.table.core.windows.net",
                ["maxmind_userid"] = "12345",
                ["maxmind_apikey"] = "fake-api-key",
                ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace repository dependencies with mocks
            services.RemoveAll<IMaxMindGeoLocationRepository>();
            services.AddSingleton(MockMaxMind.Object);

            services.RemoveAll<ITableStorageGeoLocationRepository>();
            services.AddSingleton(MockTableStorage.Object);

            // Replace TableServiceClient with a mock to avoid Azure connection
            services.RemoveAll<TableServiceClient>();
            services.AddSingleton(Mock.Of<TableServiceClient>());

            // Provide a stub TelemetryConfiguration with connection string so AppInsights/Profiler services can resolve
            services.RemoveAll<TelemetryConfiguration>();
            var telemetryConfig = new TelemetryConfiguration
            {
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost"
            };
            services.AddSingleton(telemetryConfig);

            // Remove ServiceProfiler hosted services to avoid CryptographicException
            // when multiple test factories run in parallel
            var profilerDescriptors = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                            d.ImplementationType?.FullName?.Contains("Profiler") == true)
                .ToList();
            foreach (var descriptor in profilerDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove real health checks that fail without live Azure/MaxMind services
            services.RemoveAll<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck>();

            // Replace authentication with test scheme
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });

        builder.UseEnvironment("Development");
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "LookupApiUser"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
