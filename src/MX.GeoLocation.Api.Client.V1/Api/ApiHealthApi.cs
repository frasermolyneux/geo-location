using Microsoft.Extensions.Logging;

using MX.GeoLocation.Abstractions.Interfaces;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Abstractions;
using MX.Api.Client.Extensions;

using RestSharp;

namespace MX.GeoLocation.Api.Client.V1
{
    /// <summary>
    /// Client for the API health endpoint
    /// </summary>
    public class ApiHealthApi : BaseApi<GeoLocationApiClientOptions>, IApiHealthApi
    {
        public ApiHealthApi(
            ILogger<BaseApi<GeoLocationApiClientOptions>> logger,
            IApiTokenProvider? apiTokenProvider,
            IRestClientService restClientService,
            GeoLocationApiClientOptions options)
            : base(logger, apiTokenProvider, restClientService, options)
        {
        }

        public async Task<ApiResult> CheckHealth(CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync("v1/health", Method.Get, cancellationToken);
                var response = await ExecuteAsync(request, cancellationToken);

                var result = response.ToApiResult();
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse(
                    new ApiError("CLIENT_ERROR", "Failed to check API health"));
                return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }
    }
}
