using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Api.Client.V1
{
    /// <summary>
    /// Provides version-specific access to the GeoLookup API endpoints
    /// </summary>
    public class VersionedGeoLookupApi : IVersionedGeoLookupApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedGeoLookupApi"/> class
        /// </summary>
        /// <param name="v1">The V1 GeoLookup API implementation</param>
        public VersionedGeoLookupApi(IGeoLookupApi v1)
        {
            V1 = v1;
        }

        /// <summary>
        /// Gets the V1 GeoLookup API implementation
        /// </summary>
        public IGeoLookupApi V1 { get; }
    }
}
