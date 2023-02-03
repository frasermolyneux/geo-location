using MX.GeoLocation.LookupApi.Abstractions.Models;

using MxIO.ApiClient.Abstractions;

namespace MX.GeoLocation.LookupApi.Abstractions.Interfaces
{
    public interface IGeoLookupApi
    {
        Task<ApiResponseDto<GeoLocationDto>> GetGeoLocation(string hostname);
        Task<ApiResponseDto<GeoLocationCollectionDto>> GetGeoLocations(List<string> hostnames);

        Task<ApiResponseDto> DeleteMetadata(string hostname);
    }
}
