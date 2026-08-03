using System;

using MX.Api.Client.Configuration;

namespace MX.GeoLocation.Api.Client.V1;

/// <summary>
/// Builder for GeoLocation API options
/// </summary>
public class GeoLocationApiOptionsBuilder : ApiClientOptionsBuilder<GeoLocationApiClientOptions, GeoLocationApiOptionsBuilder>
{
    /// <summary>
    /// Creates a new instance of the GeoLocationApiOptionsBuilder
    /// </summary>
    public GeoLocationApiOptionsBuilder() : base() { }

    /// <summary>
    /// Gets the cache configuration captured for later shared-scope application across the multiple
    /// typed sub-API clients registered by <see cref="ServiceCollectionExtensions.AddGeoLocationApiClient"/>.
    /// </summary>
    internal Action<CacheBuilder>? CapturedCacheConfigure { get; private set; }

    /// <summary>
    /// Captures the caller's cache configuration without applying it to this builder's client scope.
    /// The captured delegate is later re-applied per typed sub-API via
    /// <see cref="ApiClientOptionsBuilder{TOptions, TBuilder}.WithSharedCaching(SharedCacheConfiguration)"/>,
    /// which skips (rather than throws on) operations targeting sibling sub-APIs.
    /// </summary>
    /// <param name="configure">The cache policy configuration callback.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public new GeoLocationApiOptionsBuilder WithCaching(Action<CacheBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        CapturedCacheConfigure = configure;
        return this;
    }
}
