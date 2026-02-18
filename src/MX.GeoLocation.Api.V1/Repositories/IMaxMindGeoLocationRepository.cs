using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface IMaxMindGeoLocationRepository
    {
        Task<GeoLocationDto> GetGeoLocation(string address, CancellationToken cancellationToken = default);
        Task<CityGeoLocationDto> GetCityGeoLocation(string address, CancellationToken cancellationToken = default);
        Task<InsightsGeoLocationDto> GetInsightsGeoLocation(string address, CancellationToken cancellationToken = default);
    }
}