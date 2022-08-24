using RestSharp;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public interface IRestClientSingleton
    {
        void ConfigureBaseUrl(string baseUrl);
        Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);
    }
}
