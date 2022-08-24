using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MX.GeoLocation.GeoLocationApi.Client.Extensions;
using MX.GeoLocation.LookupApi.Abstractions.Interfaces;
using MX.GeoLocation.LookupApi.Abstractions.Models;

using RestSharp;

namespace MX.GeoLocation.GeoLocationApi.Client.Api
{
    public class GeoLookupApi : BaseApi, IGeoLookupApi
    {
        public GeoLookupApi(ILogger<GeoLookupApi> logger, IOptions<GeoLocationApiClientOptions> options, IApiTokenProvider apiTokenProvider, IRestClientSingleton restClientSingletonFactory) : base(logger, options, apiTokenProvider, restClientSingletonFactory)
        {
        }

        public async Task<ApiResponseDto<GeoLocationDto>> GetGeoLocation(string hostname)
        {
            var request = await CreateRequest($"lookup/{hostname}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GeoLocationDto>();
        }

        public async Task<ApiResponseDto<GeoLocationCollectionDto>> GetGeoLocations(List<string> hostnames)
        {
            var request = await CreateRequest($"lookup", Method.Post);
            request.AddJsonBody(hostnames);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GeoLocationCollectionDto>();
        }

        public async Task<ApiResponseDto> DeleteMetadata(string hostname)
        {
            var request = await CreateRequest($"lookup/{hostname}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
