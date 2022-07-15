using MX.GeoLocation.LookupApi.Abstractions.Models;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface ITableStorageGeoLocationRepository
    {
        Task<GeoLocationDto?> GetGeoLocation(string address);
        Task StoreGeoLocation(GeoLocationDto geoLocationDto);
    }
}
