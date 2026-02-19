using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Api.Client.V1;

using V1_1 = MX.GeoLocation.Abstractions.Interfaces.V1_1;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// In-memory fake of <see cref="IVersionedGeoLookupApi"/> composing the V1 and V1.1 fakes.
/// </summary>
public class FakeVersionedGeoLookupApi : IVersionedGeoLookupApi
{
    public FakeVersionedGeoLookupApi(FakeGeoLookupApi v1, FakeGeoLookupApiV1_1 v1_1)
    {
        V1 = v1;
        V1_1 = v1_1;
    }

    public IGeoLookupApi V1 { get; }
    public V1_1.IGeoLookupApi V1_1 { get; }
}

/// <summary>
/// In-memory fake of <see cref="IGeoLocationApiClient"/> for unit and integration tests.
/// Eliminates the need for nested mock hierarchies.
/// </summary>
/// <example>
/// <code>
/// // Unit test usage:
/// var fakeClient = new FakeGeoLocationApiClient();
/// fakeClient.V1Lookup.AddResponse("8.8.8.8",
///     GeoLocationDtoFactory.CreateGeoLocation(cityName: "Mountain View"));
///
/// // Integration test DI usage:
/// services.AddFakeGeoLocationApiClient(client =>
///     client.V1Lookup.AddResponse("8.8.8.8",
///         GeoLocationDtoFactory.CreateGeoLocation(cityName: "Mountain View")));
/// </code>
/// </example>
public class FakeGeoLocationApiClient : IGeoLocationApiClient
{
    /// <summary>
    /// The V1 geo-lookup fake. Use to configure canned responses.
    /// </summary>
    public FakeGeoLookupApi V1Lookup { get; } = new();

    /// <summary>
    /// The V1.1 geo-lookup fake. Use to configure canned city/insights responses.
    /// </summary>
    public FakeGeoLookupApiV1_1 V1_1Lookup { get; } = new();

    /// <summary>
    /// The API info fake. Use to configure version info responses.
    /// </summary>
    public FakeApiInfoApi InfoApi { get; } = new();

    /// <summary>
    /// The API health fake. Use to configure health check responses.
    /// </summary>
    public FakeApiHealthApi HealthApi { get; } = new();

    private readonly Lazy<FakeVersionedGeoLookupApi> _geoLookup;

    public FakeGeoLocationApiClient()
    {
        _geoLookup = new Lazy<FakeVersionedGeoLookupApi>(() => new FakeVersionedGeoLookupApi(V1Lookup, V1_1Lookup));
    }

    public IVersionedGeoLookupApi GeoLookup => _geoLookup.Value;
    public IApiInfoApi ApiInfo => InfoApi;
    public IApiHealthApi ApiHealth => HealthApi;
}
