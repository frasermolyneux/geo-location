using Microsoft.Extensions.DependencyInjection;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;

using MX.Api.Client.Extensions;

namespace MX.GeoLocation.Api.Client.V1
{
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
            // Register V1 API using the new typed API client pattern
            serviceCollection.AddTypedApiClient<IGeoLookupApi, GeoLookupApi, GeoLocationApiClientOptions, GeoLocationApiOptionsBuilder>(configureOptions);

            // Register versioned API wrapper
            serviceCollection.AddScoped<IVersionedGeoLookupApi, VersionedGeoLookupApi>();

            // Register main client
            serviceCollection.AddScoped<IGeoLocationApiClient, GeoLocationApiClient>();

            return serviceCollection;
        }
    }
}