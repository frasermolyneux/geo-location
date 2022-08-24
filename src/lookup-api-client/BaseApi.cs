using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using System.Net;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public class BaseApi
    {
        private readonly string _apimSubscriptionKey;
        private readonly IRestClientSingleton restClientSingleton;

        public BaseApi(ILogger logger, IOptions<GeoLocationApiClientOptions> options, IApiTokenProvider apiTokenProvider, IRestClientSingleton restClientSingletonFactory)
        {
            if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
                throw new ArgumentNullException(nameof(options.Value.BaseUrl));

            if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
                throw new ArgumentNullException(nameof(options.Value.ApiKey));

            _apimSubscriptionKey = options.Value.ApiKey;

            Logger = logger;
            ServersApiTokenProvider = apiTokenProvider;

            this.restClientSingleton = restClientSingletonFactory;

            if (string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix))
            {
                restClientSingletonFactory.ConfigureBaseUrl(options.Value.BaseUrl);
            }
            else
            {
                restClientSingletonFactory.ConfigureBaseUrl($"{options.Value.BaseUrl}/{options.Value.ApiPathPrefix}");
            }
        }

        public ILogger Logger { get; }
        public IApiTokenProvider ServersApiTokenProvider { get; }

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
            var response = await restClientSingleton.ExecuteAsync(request);

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