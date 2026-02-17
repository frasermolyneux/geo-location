using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface ITableStorageGeoLocationRepository
    {
        Task<GeoLocationDto?> GetGeoLocation(string address);
        Task StoreGeoLocation(GeoLocationDto geoLocationDto);
        Task<bool> DeleteGeoLocation(string address);

        Task<CityGeoLocationDto?> GetCityGeoLocation(string address);
        Task StoreCityGeoLocation(CityGeoLocationDto dto);

        Task<InsightsGeoLocationDto?> GetInsightsGeoLocation(string address, TimeSpan maxAge);
        Task StoreInsightsGeoLocation(InsightsGeoLocationDto dto);
    }
}
