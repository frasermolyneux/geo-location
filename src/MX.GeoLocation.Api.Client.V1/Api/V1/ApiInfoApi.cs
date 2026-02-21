using Microsoft.Extensions.Logging;

using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Models;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Abstractions;
using MX.Api.Client.Extensions;

using RestSharp;

namespace MX.GeoLocation.Api.Client.V1
{
    /// <summary>
    /// Client for the API info endpoint
    /// </summary>
    public class ApiInfoApi : BaseApi<GeoLocationApiClientOptions>, IApiInfoApi
    {
        public ApiInfoApi(
            ILogger<BaseApi<GeoLocationApiClientOptions>> logger,
            IApiTokenProvider? apiTokenProvider,
            IRestClientService restClientService,
            GeoLocationApiClientOptions options)
            : base(logger, apiTokenProvider, restClientService, options)
        {
        }

        public async Task<ApiResult<ApiInfoDto>> GetApiInfo(CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync("v1/info", Method.Get, cancellationToken);
                var response = await ExecuteAsync(request, cancellationToken);

                var result = response.ToApiResult<ApiInfoDto>();
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse<ApiInfoDto>(
                    new ApiError("CLIENT_ERROR", "Failed to retrieve API info"));
                return new ApiResult<ApiInfoDto>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }
    }
}
