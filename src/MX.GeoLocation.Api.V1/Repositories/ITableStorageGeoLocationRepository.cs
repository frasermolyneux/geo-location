using MX.GeoLocation.Abstractions.Models.V1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface ITableStorageGeoLocationRepository
    {
        Task<GeoLocationDto?> GetGeoLocation(string address);
        Task StoreGeoLocation(GeoLocationDto geoLocationDto);
    }
}
