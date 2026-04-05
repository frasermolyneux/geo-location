using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface IProxyCheckCacheRepository
    {
        Task<ProxyCheckDto?> GetProxyCheckData(string address, TimeSpan maxAge, CancellationToken cancellationToken = default);
        Task StoreProxyCheckData(ProxyCheckDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteProxyCheckData(string address, CancellationToken cancellationToken = default);
    }
}
