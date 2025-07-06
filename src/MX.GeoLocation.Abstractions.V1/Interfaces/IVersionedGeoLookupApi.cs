using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Abstractions.Interfaces
{
    /// <summary>
    /// Provides versioned access to geo lookup APIs
    /// </summary>
    public interface IVersionedGeoLookupApi
    {
        /// <summary>
        /// Gets the V1 implementation of the geo lookup API
        /// </summary>
        IGeoLookupApi V1 { get; }
    }
}
