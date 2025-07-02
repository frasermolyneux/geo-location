using MX.GeoLocation.Abstractions.Models.V1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface IMaxMindGeoLocationRepository
    {
        Task<GeoLocationDto> GetGeoLocation(string address);
    }
}