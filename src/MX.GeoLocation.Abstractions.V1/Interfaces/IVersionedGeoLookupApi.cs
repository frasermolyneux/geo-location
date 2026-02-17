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
        V1.IGeoLookupApi V1 { get; }

        /// <summary>
        /// Gets the V1.1 implementation of the geo lookup API
        /// </summary>
        V1_1.IGeoLookupApi V1_1 { get; }
    }
}
