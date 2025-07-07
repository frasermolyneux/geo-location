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
        /// Adds the GeoLocation API client services to the service collection
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddGeoLocationApiClient(this IServiceCollection serviceCollection,
            Action<GeoLocationApiClientOptions>? configure = null)
        {
            serviceCollection.AddApiClient();

            if (configure != null)
            {
                serviceCollection.Configure(configure);
            }

            // Register the V1 API implementation as scoped to match IRestClientService lifetime
            serviceCollection.AddScoped<IGeoLookupApi, GeoLookupApi>();

            // Register the versioned API selector as scoped to match V1 API lifetime
            serviceCollection.AddScoped<IVersionedGeoLookupApi, VersionedGeoLookupApi>();

            // Register the main API client as scoped to match versioned API lifetime
            serviceCollection.AddScoped<IGeoLocationApiClient, GeoLocationApiClient>();

            return serviceCollection;
        }
    }
}