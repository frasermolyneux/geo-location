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
        /// <param name="apiInfoApi">The versioned API info endpoint</param>
        /// <param name="apiHealthApi">The versioned API health endpoint</param>
        public GeoLocationApiClient(IVersionedGeoLookupApi versionedGeoLookupApi, IVersionedApiInfoApi apiInfoApi, IVersionedApiHealthApi apiHealthApi)
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
        /// Gets the versioned API info endpoint
        /// </summary>
        public IVersionedApiInfoApi ApiInfo { get; }

        /// <summary>
        /// Gets the versioned API health endpoint
        /// </summary>
        public IVersionedApiHealthApi ApiHealth { get; }
    }
}