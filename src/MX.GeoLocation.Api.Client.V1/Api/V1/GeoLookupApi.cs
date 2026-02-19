using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;
using MX.Api.Abstractions;
using MX.Api.Client.Extensions;

using RestSharp;

namespace MX.GeoLocation.Api.Client.V1
{
    public class GeoLookupApi : BaseApi<GeoLocationApiClientOptions>, IGeoLookupApi
    {
        public GeoLookupApi(
            ILogger<BaseApi<GeoLocationApiClientOptions>> logger,
            IApiTokenProvider? apiTokenProvider,
            IRestClientService restClientService,
            GeoLocationApiClientOptions options)
            : base(logger, apiTokenProvider, restClientService, options)
        {
        }

        public async Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync($"v1/lookup/{Uri.EscapeDataString(hostname)}", Method.Get, cancellationToken);
                var response = await ExecuteAsync(request, cancellationToken);

                var result = response.ToApiResult<GeoLocationDto>();
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse<GeoLocationDto>(
                    new ApiError("CLIENT_ERROR", "Failed to retrieve geolocation"));
                return new ApiResult<GeoLocationDto>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        public async Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync($"v1/lookup", Method.Post, cancellationToken);
                if (hostnames is not null)
                {
                    request.AddJsonBody(hostnames);
                }

                var response = await ExecuteAsync(request, cancellationToken);
                var result = response.ToApiResult<CollectionModel<GeoLocationDto>>();

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse<CollectionModel<GeoLocationDto>>(
                    new ApiError("CLIENT_ERROR", "Failed to retrieve geolocations"));
                return new ApiResult<CollectionModel<GeoLocationDto>>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        public async Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await CreateRequestAsync($"v1/lookup/{Uri.EscapeDataString(hostname)}", Method.Delete, cancellationToken);
                var response = await ExecuteAsync(request, cancellationToken);

                var result = response.ToApiResult();
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var errorResponse = new ApiResponse(
                    new ApiError("CLIENT_ERROR", "Failed to delete metadata"));
                return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }
    }
}
