using MX.GeoLocation.Abstractions.Models.V1;

using MX.Api.Abstractions;

namespace MX.GeoLocation.Abstractions.Interfaces.V1
{
    /// <summary>
    /// Interface for GeoLookup API version 1 operations
    /// </summary>
    public interface IGeoLookupApi
    {
        /// <summary>
        /// Gets geolocation information for a single hostname or IP address
        /// </summary>
        /// <param name="hostname">The hostname or IP address to lookup</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Geolocation information for the hostname</returns>
        Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets geolocation information for multiple hostnames or IP addresses
        /// </summary>
        /// <param name="hostnames">List of hostnames or IP addresses to lookup</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of geolocation information</returns>
        Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes cached metadata for a hostname or IP address
        /// </summary>
        /// <param name="hostname">The hostname or IP address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the delete operation</returns>
        Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default);
    }
}
