using Microsoft.Extensions.Logging;

using MX.GeoLocation.Abstractions.Interfaces.V1_1;
using MX.GeoLocation.Abstractions.Models.V1_1;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Abstractions;
using MX.Api.Client.Extensions;

using RestSharp;

namespace MX.GeoLocation.Api.Client.V1
{
    public class GeoLookupApiV1_1 : BaseApi<GeoLocationApiClientOptions>, IGeoLookupApi
    {
        public GeoLookupApiV1_1(
            ILogger<BaseApi<GeoLocationApiClientOptions>> logger,
            IApiTokenProvider? apiTokenProvider,
            IRestClientService restClientService,
            GeoLocationApiClientOptions options)
            : base(logger, apiTokenProvider, restClientService, options)
        {
        }

        public async Task<ApiResult<CityGeoLocationDto>> GetCityGeoLocation(string hostname, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync($"v1.1/lookup/city/{hostname}", Method.Get, cancellationToken);
                var response = await ExecuteAsync(request, cancellationToken);

                return response.ToApiResult<CityGeoLocationDto>();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse<CityGeoLocationDto>(
                    new ApiError("CLIENT_ERROR", "Failed to retrieve city geolocation"));
                return new ApiResult<CityGeoLocationDto>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        public async Task<ApiResult<InsightsGeoLocationDto>> GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync($"v1.1/lookup/insights/{hostname}", Method.Get, cancellationToken);
                var response = await ExecuteAsync(request, cancellationToken);

                return response.ToApiResult<InsightsGeoLocationDto>();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse<InsightsGeoLocationDto>(
                    new ApiError("CLIENT_ERROR", "Failed to retrieve insights geolocation"));
                return new ApiResult<InsightsGeoLocationDto>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }
    }
}
