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
    public class GeoLookupApi : BaseApi, IGeoLookupApi
    {
        public GeoLookupApi(ILogger<GeoLookupApi> logger, IApiTokenProvider? apiTokenProvider, IRestClientService restClientService, IOptions<GeoLocationApiClientOptions> options) : base(logger, apiTokenProvider, restClientService, options)
        {

        }

        public async Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname)
        {
            var request = await CreateRequestAsync($"v1/lookup/{hostname}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResult<GeoLocationDto>();
        }

        public async Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames)
        {
            var request = await CreateRequestAsync($"v1/lookup", Method.Post);
            request.AddJsonBody(hostnames);

            var response = await ExecuteAsync(request);

            return response.ToApiResult<CollectionModel<GeoLocationDto>>();
        }

        public async Task<ApiResult> DeleteMetadata(string hostname)
        {
            var request = await CreateRequestAsync($"v1/lookup/{hostname}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResult();
        }
    }
}
