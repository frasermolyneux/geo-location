using System;

using Microsoft.Extensions.DependencyInjection;

using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;

using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Api.Client.V1.Caching;

namespace MX.GeoLocation.Api.Client.V1;

/// <summary>
/// Extension methods for configuring GeoLocation API client services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the GeoLocation API client services with custom configuration
    /// </summary>
    /// <param name="serviceCollection">The service collection</param>
    /// <param name="configureOptions">Action to configure the GeoLocation API options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGeoLocationApiClient(
        this IServiceCollection serviceCollection,
        Action<GeoLocationApiOptionsBuilder> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Probe the caller-supplied delegate once against a throwaway builder to capture any
        // cache configuration without binding it to a single typed sub-API scope. The overridden
        // GeoLocationApiOptionsBuilder.WithCaching stashes the delegate on CapturedCacheConfigure
        // instead of invoking it directly (which would fail scope validation when expressions
        // target multiple sub-API interfaces).
        var probe = new GeoLocationApiOptionsBuilder();
        configureOptions(probe);
        var capturedCache = probe.CapturedCacheConfigure;
        var sharedCache = capturedCache is null ? null : new SharedCacheConfiguration(capturedCache);

        var perClient = BuildPerClientConfigurator(configureOptions, sharedCache);

        // Register library cache defaults for the read-only lookup surfaces BEFORE the typed
        // clients so DefaultCachePolicies<TClient> singletons are visible during construction.
        // Consumers opt in via WithCaching(c => c.UseLibraryDefaults()).
        serviceCollection.AddDefaultCachePolicies<IGeoLookupApi>(GeoLocationApiCacheDefaults.ConfigureV1);
        serviceCollection.AddDefaultCachePolicies<Abstractions.Interfaces.V1_1.IGeoLookupApi>(GeoLocationApiCacheDefaults.ConfigureV1_1);

        // Register V1 API using the new typed API client pattern
        serviceCollection.AddTypedApiClient<IGeoLookupApi, GeoLookupApi, GeoLocationApiClientOptions, GeoLocationApiOptionsBuilder>(perClient);

        // Register V1.1 API
        serviceCollection.AddTypedApiClient<Abstractions.Interfaces.V1_1.IGeoLookupApi, GeoLookupApiV1_1, GeoLocationApiClientOptions, GeoLocationApiOptionsBuilder>(perClient);

        // Register API info endpoint
        serviceCollection.AddTypedApiClient<IApiInfoApi, ApiInfoApi, GeoLocationApiClientOptions, GeoLocationApiOptionsBuilder>(perClient);

        // Register API health endpoint
        serviceCollection.AddTypedApiClient<IApiHealthApi, ApiHealthApi, GeoLocationApiClientOptions, GeoLocationApiOptionsBuilder>(perClient);

        // Fail fast at registration time if a captured cache expression targets an interface that
        // is not assignable from any registered typed sub-API (typo guard).
        sharedCache?.ValidateAllOperationsMatched();

        // Register version selectors as scoped
        serviceCollection.AddScoped<IVersionedGeoLookupApi, VersionedGeoLookupApi>();
        serviceCollection.AddScoped<IVersionedApiHealthApi, VersionedApiHealthApi>();
        serviceCollection.AddScoped<IVersionedApiInfoApi, VersionedApiInfoApi>();

        // Register the unified client as scoped
        serviceCollection.AddScoped<IGeoLocationApiClient, GeoLocationApiClient>();

        return serviceCollection;
    }

    private static Action<GeoLocationApiOptionsBuilder> BuildPerClientConfigurator(
        Action<GeoLocationApiOptionsBuilder> configureOptions,
        SharedCacheConfiguration? sharedCache)
    {
        return sharedCache is null
            ? configureOptions
            : builder =>
            {
                configureOptions(builder);
                builder.WithSharedCaching(sharedCache);
            };
    }
}
