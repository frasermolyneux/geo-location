
using Azure.Data.Tables;

using MX.GeoLocation.LookupApi.Abstractions.Models;
using MX.GeoLocation.LookupWebApi.Models;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public class TableStorageGeoLocationRepository : ITableStorageGeoLocationRepository
    {
        private readonly TableClient tableClient;

        public TableStorageGeoLocationRepository(IConfiguration configuration)
        {
            tableClient = new TableClient(configuration["appdata_storage_connectionstring"], "geolocations");
        }

        public async Task<GeoLocationDto> GetGeoLocation(string address)
        {
            var entry = await tableClient.GetEntityAsync<GeoLocationTableEntity>("addresses", address);
            return entry;
        }

        public async Task StoreGeoLocation(GeoLocationDto geoLocationDto)
        {
            var entity = new GeoLocationTableEntity(geoLocationDto);
            await tableClient.AddEntityAsync(entity);
        }
    }
}
