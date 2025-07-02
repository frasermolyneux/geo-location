
using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.Abstractions.Models.V1;
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

        public async Task<GeoLocationDto?> GetGeoLocation(string address)
        {
            try
            {
                var entry = await tableClient.GetEntityAsync<GeoLocationTableEntity>("addresses", address);
                return entry.Value.GeoLocationDto();
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode != "ResourceNotFound")
                    throw;
            }

            return null;
        }

        public async Task StoreGeoLocation(GeoLocationDto geoLocationDto)
        {
            var entity = new GeoLocationTableEntity(geoLocationDto);
            await tableClient.AddEntityAsync(entity);
        }
    }
}
