using MX.GeoLocation.Abstractions.Models.V1;

using MX.Api.Abstractions;

namespace MX.GeoLocation.Abstractions.Interfaces.V1
{
    public interface IGeoLookupApi
    {
        Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname);
        Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames);

        Task<ApiResult> DeleteMetadata(string hostname);
    }
}
