using Microsoft.Extensions.DependencyInjection;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Api.Client.Testing;
using MX.GeoLocation.Api.Client.V1;

using V1_1 = MX.GeoLocation.Abstractions.Interfaces.V1_1;

namespace MX.GeoLocation.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFakeGeoLocationApiClient_RegistersAllServices()
    {
        var services = new ServiceCollection();

        services.AddFakeGeoLocationApiClient(client =>
        {
            client.V1Lookup.AddResponse("8.8.8.8",
                GeoLocationDtoFactory.CreateGeoLocation(cityName: "Mountain View"));
        });

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IGeoLocationApiClient>());
        Assert.NotNull(provider.GetRequiredService<IVersionedGeoLookupApi>());
        Assert.NotNull(provider.GetRequiredService<IGeoLookupApi>());
        Assert.NotNull(provider.GetRequiredService<V1_1.IGeoLookupApi>());
        Assert.NotNull(provider.GetRequiredService<IApiInfoApi>());
        Assert.NotNull(provider.GetRequiredService<IApiHealthApi>());
    }

    [Fact]
    public async Task AddFakeGeoLocationApiClient_ConfiguredResponses_ResolveCorrectly()
    {
        var services = new ServiceCollection();

        services.AddFakeGeoLocationApiClient(client =>
        {
            client.V1Lookup.AddResponse("8.8.8.8",
                GeoLocationDtoFactory.CreateGeoLocation(cityName: "Mountain View"));
        });

        var provider = services.BuildServiceProvider();
        var apiClient = provider.GetRequiredService<IGeoLocationApiClient>();
        var result = await apiClient.GeoLookup.V1.GetGeoLocation("8.8.8.8");

        Assert.Equal("Mountain View", result.Result!.Data!.CityName);
    }

    [Fact]
    public void AddFakeGeoLocationApiClient_WithoutConfigure_RegistersWithDefaults()
    {
        var services = new ServiceCollection();

        services.AddFakeGeoLocationApiClient();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IGeoLocationApiClient>());
    }
}
