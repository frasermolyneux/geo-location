using MX.Api.Abstractions;

namespace MX.GeoLocation.LookupWebApi.Services;

public interface IGeoLookupService
{
    Task<ApiResult<T>> ExecuteLookup<T>(string hostname, CancellationToken cancellationToken, Func<string, Task<ApiResult<T>>> lookupFunc) where T : class;
}
