using MX.GeoLocation.Abstractions.Interfaces;

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
    }
}