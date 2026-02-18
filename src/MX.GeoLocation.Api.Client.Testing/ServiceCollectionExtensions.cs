using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Api.Client.V1;

using V1_1 = MX.GeoLocation.Abstractions.Interfaces.V1_1;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// DI extensions for registering fake geo-location services in integration tests.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces the real <see cref="IGeoLocationApiClient"/> and all related services
    /// with in-memory fakes. Use the optional <paramref name="configure"/> callback to
    /// set up canned responses.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddFakeGeoLocationApiClient(client =>
    /// {
    ///     client.V1Lookup.AddResponse("8.8.8.8",
    ///         GeoLocationDtoFactory.CreateGeoLocation(cityName: "Mountain View", countryName: "United States"));
    ///     client.V1_1Lookup.AddCityResponse("1.1.1.1",
    ///         GeoLocationDtoFactory.CreateCityGeoLocation(cityName: "Los Angeles"));
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddFakeGeoLocationApiClient(
        this IServiceCollection services,
        Action<FakeGeoLocationApiClient>? configure = null)
    {
        var fakeClient = new FakeGeoLocationApiClient();
        configure?.Invoke(fakeClient);

        services.RemoveAll<IGeoLocationApiClient>();
        services.RemoveAll<IVersionedGeoLookupApi>();
        services.RemoveAll<IGeoLookupApi>();
        services.RemoveAll<V1_1.IGeoLookupApi>();
        services.RemoveAll<IApiInfoApi>();
        services.RemoveAll<IApiHealthApi>();

        services.AddSingleton<IGeoLocationApiClient>(fakeClient);
        services.AddSingleton<IVersionedGeoLookupApi>(fakeClient.GeoLookup);
        services.AddSingleton<IGeoLookupApi>(fakeClient.V1Lookup);
        services.AddSingleton<V1_1.IGeoLookupApi>(fakeClient.V1_1Lookup);
        services.AddSingleton<IApiInfoApi>(fakeClient.InfoApi);
        services.AddSingleton<IApiHealthApi>(fakeClient.HealthApi);

        return services;
    }
}
