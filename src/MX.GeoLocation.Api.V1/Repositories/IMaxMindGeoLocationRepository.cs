using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface IMaxMindGeoLocationRepository
    {
        Task<GeoLocationDto> GetGeoLocation(string address);
        Task<CityGeoLocationDto> GetCityGeoLocation(string address);
        Task<InsightsGeoLocationDto> GetInsightsGeoLocation(string address);
    }
}