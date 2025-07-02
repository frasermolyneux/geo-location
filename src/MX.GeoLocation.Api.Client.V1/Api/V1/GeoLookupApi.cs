using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MX.GeoLocation.Api.Client.Extensions.V1;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;

using MxIO.ApiClient;
using MxIO.ApiClient.Abstractions;

using RestSharp;

namespace MX.GeoLocation.Api.Client.V1
{
    public class GeoLookupApi : BaseApi, IGeoLookupApi
    {
        public GeoLookupApi(ILogger<GeoLookupApi> logger, IApiTokenProvider apiTokenProvider, IRestClientSingleton restClientSingletonFactory, IOptions<GeoLocationApiClientOptions> options) : base(logger, apiTokenProvider, restClientSingletonFactory, options)
        {

        }

        public async Task<ApiResponseDto<GeoLocationDto>> GetGeoLocation(string hostname)
        {
            var request = await CreateRequestAsync($"lookup/{hostname}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GeoLocationDto>();
        }

        public async Task<ApiResponseDto<GeoLocationCollectionDto>> GetGeoLocations(List<string> hostnames)
        {
            var request = await CreateRequestAsync($"lookup", Method.Post);
            request.AddJsonBody(hostnames);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GeoLocationCollectionDto>();
        }

        public async Task<ApiResponseDto> DeleteMetadata(string hostname)
        {
            var request = await CreateRequestAsync($"lookup/{hostname}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
