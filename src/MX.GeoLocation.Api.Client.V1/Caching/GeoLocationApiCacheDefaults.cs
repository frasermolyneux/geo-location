using System;

using MX.Api.Abstractions;
using MX.Api.Client.Configuration;

using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

using V1_1 = MX.GeoLocation.Abstractions.Interfaces.V1_1;

namespace MX.GeoLocation.Api.Client.V1.Caching;

/// <summary>
/// Default library cache policies for the read-only GeoLocation lookup surface.
/// Consumers opt in by calling <c>WithCaching(c =&gt; c.UseLibraryDefaults())</c> when
/// registering <see cref="ServiceCollectionExtensions.AddGeoLocationApiClient"/>.
/// </summary>
/// <remarks>
/// Only GET-style lookups are cached; write / delete / info / health operations are
/// intentionally left uncached. TTLs favour freshness for volatile signals
/// (ProxyCheck / IP intelligence) and stability for geographic mappings.
/// </remarks>
public static class GeoLocationApiCacheDefaults
{
    /// <summary>Default in-memory TTL for MaxMind v1.0 geolocation lookups.</summary>
    public static readonly TimeSpan V1GeoLocationTtl = TimeSpan.FromMinutes(60);

    /// <summary>Default in-memory TTL for MaxMind v1.1 city lookups.</summary>
    public static readonly TimeSpan CityTtl = TimeSpan.FromMinutes(60);

    /// <summary>Default in-memory TTL for MaxMind v1.1 insights lookups.</summary>
    public static readonly TimeSpan InsightsTtl = TimeSpan.FromMinutes(30);

    /// <summary>Default in-memory TTL for ProxyCheck.io risk lookups (short — risk signals change).</summary>
    public static readonly TimeSpan ProxyCheckTtl = TimeSpan.FromMinutes(15);

    /// <summary>Default in-memory TTL for merged IP intelligence lookups (bounded by ProxyCheck freshness).</summary>
    public static readonly TimeSpan IntelligenceTtl = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Configures cache defaults for <see cref="IGeoLookupApi"/> (v1.0). Only single-item GET is cached;
    /// batch POST and DELETE are left uncached.
    /// </summary>
    public static void ConfigureV1(CacheBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.InMemory<IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<GeoLocationDto>>>(
            api => api.GetGeoLocation(default!, default),
            V1GeoLocationTtl);
    }

    /// <summary>
    /// Configures cache defaults for <see cref="V1_1.IGeoLookupApi"/> (v1.1). Only single-item GET lookups
    /// are cached; batch intelligence POST and DELETE are left uncached.
    /// </summary>
    public static void ConfigureV1_1(CacheBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.InMemory<V1_1.IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<CityGeoLocationDto>>>(
            api => api.GetCityGeoLocation(default!, default),
            CityTtl);

        builder.InMemory<V1_1.IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<InsightsGeoLocationDto>>>(
            api => api.GetInsightsGeoLocation(default!, default),
            InsightsTtl);

        builder.InMemory<V1_1.IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<ProxyCheckDto>>>(
            api => api.GetProxyCheck(default!, default),
            ProxyCheckTtl);

        builder.InMemory<V1_1.IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<IpIntelligenceDto>>>(
            api => api.GetIpIntelligence(default!, default),
            IntelligenceTtl);
    }
}
