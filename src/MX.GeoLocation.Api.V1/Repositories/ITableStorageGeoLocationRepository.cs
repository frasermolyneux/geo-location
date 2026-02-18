using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface ITableStorageGeoLocationRepository
    {
        Task<GeoLocationDto?> GetGeoLocation(string address, CancellationToken cancellationToken = default);
        Task StoreGeoLocation(GeoLocationDto geoLocationDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteGeoLocation(string address, CancellationToken cancellationToken = default);

        Task<CityGeoLocationDto?> GetCityGeoLocation(string address, CancellationToken cancellationToken = default);
        Task StoreCityGeoLocation(CityGeoLocationDto dto, CancellationToken cancellationToken = default);

        Task<InsightsGeoLocationDto?> GetInsightsGeoLocation(string address, TimeSpan maxAge, CancellationToken cancellationToken = default);
        Task StoreInsightsGeoLocation(InsightsGeoLocationDto dto, CancellationToken cancellationToken = default);
    }
}
