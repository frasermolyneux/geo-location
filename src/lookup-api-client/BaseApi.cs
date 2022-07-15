using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public class BaseApi
    {
        private readonly string _apimSubscriptionKey;

        public BaseApi(ILogger logger, IOptions<GeoLocationApiClientOptions> options, IApiTokenProvider serversApiTokenProvider)
        {
            _apimSubscriptionKey = options.Value.ApimSubscriptionKey;

            RestClient = string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix)
                ? new RestClient($"{options.Value.ApimBaseUrl}")
                : new RestClient($"{options.Value.ApimBaseUrl}/{options.Value.ApiPathPrefix}");

            Logger = logger;
            ServersApiTokenProvider = serversApiTokenProvider;
        }

        public ILogger Logger { get; }
        public IApiTokenProvider ServersApiTokenProvider { get; }
        private RestClient RestClient { get; }

        public async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var accessToken = await ServersApiTokenProvider.GetAccessToken();

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", _apimSubscriptionKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var response = await RestClient.ExecuteAsync(request);

            if (response.ErrorException != null)
            {
                Logger.LogError(response.ErrorException, "Failed {method} to '{resource}' with code '{statusCode}'", request.Method, request.Resource, response.StatusCode);
                throw response.ErrorException;
            }

            if (new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Contains(response.StatusCode))
            {
                return response;
            }
            else
            {
                Logger.LogError("Failed {method} to '{resource}' with response status '{responseStatus}' and code '{statusCode}'", request.Method, request.Resource, response.ResponseStatus, response.StatusCode);
                throw new Exception($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
            }
        }
    }
}