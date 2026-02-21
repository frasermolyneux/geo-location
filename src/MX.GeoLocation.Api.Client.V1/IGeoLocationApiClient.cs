namespace MX.GeoLocation.Api.Client.V1
{
    /// <summary>
    /// Interface for the GeoLocation API client
    /// </summary>
    public interface IGeoLocationApiClient
    {
        /// <summary>
        /// Gets the versioned GeoLookup API
        /// </summary>
        IVersionedGeoLookupApi GeoLookup { get; }

        /// <summary>
        /// Gets the versioned API info endpoint
        /// </summary>
        IVersionedApiInfoApi ApiInfo { get; }

        /// <summary>
        /// Gets the versioned API health endpoint
        /// </summary>
        IVersionedApiHealthApi ApiHealth { get; }
    }
}