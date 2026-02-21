using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Api.Client.V1
{
    // Implementation classes for version selectors
    public class VersionedGeoLookupApi : IVersionedGeoLookupApi
    {
        public VersionedGeoLookupApi(IGeoLookupApi v1, Abstractions.Interfaces.V1_1.IGeoLookupApi v1_1)
        {
            V1 = v1;
            V1_1 = v1_1;
        }

        public IGeoLookupApi V1 { get; }
        public Abstractions.Interfaces.V1_1.IGeoLookupApi V1_1 { get; }
    }

    public class VersionedApiHealthApi : IVersionedApiHealthApi
    {
        public VersionedApiHealthApi(IApiHealthApi v1)
        {
            V1 = v1;
        }

        public IApiHealthApi V1 { get; }
    }

    public class VersionedApiInfoApi : IVersionedApiInfoApi
    {
        public VersionedApiInfoApi(IApiInfoApi v1)
        {
            V1 = v1;
        }

        public IApiInfoApi V1 { get; }
    }
}
