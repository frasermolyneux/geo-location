using MX.GeoLocation.Abstractions.Interfaces;

namespace MX.GeoLocation.Api.Client.V1
{
    /// <summary>
    /// Implementation of the GeoLocation API client that provides access to versioned API endpoints
    /// </summary>
    public class GeoLocationApiClient : IGeoLocationApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoLocationApiClient"/> class
        /// </summary>
        /// <param name="versionedGeoLookupApi">The versioned GeoLookup API</param>
        /// <param name="apiInfoApi">The API info endpoint</param>
        /// <param name="apiHealthApi">The API health endpoint</param>
        public GeoLocationApiClient(IVersionedGeoLookupApi versionedGeoLookupApi, IApiInfoApi apiInfoApi, IApiHealthApi apiHealthApi)
        {
            GeoLookup = versionedGeoLookupApi;
            ApiInfo = apiInfoApi;
            ApiHealth = apiHealthApi;
        }

        /// <summary>
        /// Gets the versioned GeoLookup API
        /// </summary>
        public IVersionedGeoLookupApi GeoLookup { get; }

        /// <summary>
        /// Gets the API info endpoint
        /// </summary>
        public IApiInfoApi ApiInfo { get; }

        /// <summary>
        /// Gets the API health endpoint
        /// </summary>
        public IApiHealthApi ApiHealth { get; }
    }
}