using MX.GeoLocation.Abstractions.Models.V1;

using MxIO.ApiClient.Abstractions;

namespace MX.GeoLocation.Abstractions.Interfaces.V1
{
    public interface IGeoLookupApi
    {
        Task<ApiResponseDto<GeoLocationDto>> GetGeoLocation(string hostname);
        Task<ApiResponseDto<GeoLocationCollectionDto>> GetGeoLocations(List<string> hostnames);

        Task<ApiResponseDto> DeleteMetadata(string hostname);
    }
}
