using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public interface IProxyCheckRepository
    {
        Task<ProxyCheckDto> GetProxyCheckData(string address, CancellationToken cancellationToken = default);
    }
}
