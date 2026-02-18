using System.Net;
using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Models;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// In-memory fake of <see cref="IApiInfoApi"/> for tests.
/// </summary>
public class FakeApiInfoApi : IApiInfoApi
{
    private ApiInfoDto _info = GeoLocationDtoFactory.CreateApiInfo();

    /// <summary>
    /// Configures the API info response.
    /// </summary>
    public FakeApiInfoApi WithInfo(ApiInfoDto info)
    {
        _info = info;
        return this;
    }

    public Task<ApiResult<ApiInfoDto>> GetApiInfo(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ApiResult<ApiInfoDto>(HttpStatusCode.OK, new ApiResponse<ApiInfoDto>(_info)));
    }
}

/// <summary>
/// In-memory fake of <see cref="IApiHealthApi"/> for tests.
/// </summary>
public class FakeApiHealthApi : IApiHealthApi
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    /// <summary>
    /// Configures the health check to return a specific status code.
    /// </summary>
    public FakeApiHealthApi WithStatusCode(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    public Task<ApiResult> CheckHealth(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ApiResult(_statusCode, new ApiResponse()));
    }
}
