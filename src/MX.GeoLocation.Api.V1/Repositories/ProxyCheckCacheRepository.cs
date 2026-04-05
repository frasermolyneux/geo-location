using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Models;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public class ProxyCheckCacheRepository : IProxyCheckCacheRepository
    {
        private const string TableName = "proxycheck";
        private const string PartitionKey = "addresses";

        private readonly TableClient _tableClient;
        private readonly ILogger<ProxyCheckCacheRepository> _logger;

        public ProxyCheckCacheRepository(
            TableServiceClient tableServiceClient,
            ILogger<ProxyCheckCacheRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(tableServiceClient);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableClient = tableServiceClient.GetTableClient(TableName);
        }

        public async Task<ProxyCheckDto?> GetProxyCheckData(string address, TimeSpan maxAge, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);

            try
            {
                var response = await _tableClient.GetEntityAsync<ProxyCheckTableEntity>(
                    PartitionKey, address, cancellationToken: cancellationToken).ConfigureAwait(false);

                var entity = response.Value;

                if (entity.Timestamp.HasValue && DateTimeOffset.UtcNow - entity.Timestamp.Value > maxAge)
                {
                    _logger.LogDebug("ProxyCheck cache entry for {Address} has expired (age: {Age})", address, DateTimeOffset.UtcNow - entity.Timestamp.Value);
                    return null;
                }

                return entity.ToDto();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task StoreProxyCheckData(ProxyCheckDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TranslatedAddress);

            var entity = new ProxyCheckTableEntity(dto);
            await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> DeleteProxyCheckData(string address, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);

            try
            {
                await _tableClient.DeleteEntityAsync(PartitionKey, address, cancellationToken: cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }
    }
}
