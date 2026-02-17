using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MX.GeoLocation.Api.Client.V1;
using MX.Api.Client;
using MX.Api.Client.Auth;

namespace MX.GeoLocation.Api.Client.Tests.V1.Api.V1;

/// <summary>
/// Test to verify that the GeoLookupApi can be properly resolved from dependency injection
/// </summary>
public class DIResolutionTest
{
    [Fact]
    public void GeoLookupApi_CanBeResolvedFromDI_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add required dependencies
        services.AddSingleton<IApiTokenProvider>(Mock.Of<IApiTokenProvider>());
        services.AddSingleton<IRestClientService>(Mock.Of<IRestClientService>());

        // Add the options
        var options = new GeoLocationApiClientOptions
        {
            BaseUrl = "https://test.example.com"
        };
        services.AddSingleton(options);

        // Register the GeoLookupApi itself
        services.AddScoped<GeoLookupApi>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var geoLookupApi = serviceProvider.GetService<GeoLookupApi>();

        Assert.NotNull(geoLookupApi);
    }
}
