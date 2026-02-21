using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Api.Client.V1
{
    // Version selectors for GeoLookup API (V1 and V1.1)
    public interface IVersionedGeoLookupApi
    {
        IGeoLookupApi V1 { get; }
        Abstractions.Interfaces.V1_1.IGeoLookupApi V1_1 { get; }
    }

    public interface IVersionedApiHealthApi
    {
        IApiHealthApi V1 { get; }
    }

    public interface IVersionedApiInfoApi
    {
        IApiInfoApi V1 { get; }
    }
}
