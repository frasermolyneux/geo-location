using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Models;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public class TableStorageGeoLocationRepository : ITableStorageGeoLocationRepository
    {
        private readonly TableClient tableClient;
        private readonly TableClient v11TableClient;

        public TableStorageGeoLocationRepository(TableServiceClient tableServiceClient)
        {
            tableClient = tableServiceClient.GetTableClient("geolocations");
            v11TableClient = tableServiceClient.GetTableClient("geolocationsv11");
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
            await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        public async Task<bool> DeleteGeoLocation(string address)
        {
            try
            {
                await tableClient.DeleteEntityAsync("addresses", address);
                return true;
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode == "ResourceNotFound")
                    return false;

                throw;
            }
        }

        public async Task<CityGeoLocationDto?> GetCityGeoLocation(string address)
        {
            try
            {
                var entry = await v11TableClient.GetEntityAsync<CityGeoLocationTableEntity>("addresses", address);
                return entry.Value.ToCityDto();
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode != "ResourceNotFound")
                    throw;
            }

            return null;
        }

        public async Task StoreCityGeoLocation(CityGeoLocationDto dto)
        {
            var entity = new CityGeoLocationTableEntity(dto);
            await v11TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        public async Task<InsightsGeoLocationDto?> GetInsightsGeoLocation(string address, TimeSpan maxAge)
        {
            try
            {
                var entry = await v11TableClient.GetEntityAsync<CityGeoLocationTableEntity>("addresses", address);
                var entity = entry.Value;

                if (!entity.HasAnonymizerData)
                    return null;

                // Check if the cached entry is within the max age
                if (entity.Timestamp.HasValue && DateTimeOffset.UtcNow - entity.Timestamp.Value > maxAge)
                    return null;

                return entity.ToInsightsDto();
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode != "ResourceNotFound")
                    throw;
            }

            return null;
        }

        public async Task StoreInsightsGeoLocation(InsightsGeoLocationDto dto)
        {
            var entity = new CityGeoLocationTableEntity(dto);
            await v11TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }
    }
}
