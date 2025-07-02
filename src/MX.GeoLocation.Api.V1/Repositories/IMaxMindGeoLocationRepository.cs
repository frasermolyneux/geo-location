using MX.GeoLocation.LookupApi.Abstractions.Models;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface IMaxMindGeoLocationRepository
    {
        Task<GeoLocationDto> GetGeoLocation(string address);
    }
}