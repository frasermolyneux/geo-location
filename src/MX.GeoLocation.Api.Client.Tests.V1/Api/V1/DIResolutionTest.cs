using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MX.GeoLocation.Api.Client.V1;
using MX.Api.Client;
using MX.Api.Client.Auth;
using NUnit.Framework;
using FluentAssertions;

namespace MX.GeoLocation.Api.Client.Tests.V1.Api.V1;

/// <summary>
/// Test to verify that the GeoLookupApi can be properly resolved from dependency injection
/// </summary>
internal class DIResolutionTest
{
    [Test]
    public void GeoLookupApi_CanBeResolvedFromDI_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add required dependencies
        services.AddSingleton<IApiTokenProvider>(A.Fake<IApiTokenProvider>());
        services.AddSingleton<IRestClientService>(A.Fake<IRestClientService>());

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

        geoLookupApi.Should().NotBeNull();
    }
}
