namespace MX.GeoLocation.GeoLocationApi.Client
{
    public class GeoLocationApiClientOptions
    {
        public string ApimBaseUrl { get; set; } = string.Empty;
        public string ApimSubscriptionKey { get; set; } = string.Empty;
        public string ApiPathPrefix { get; set; } = "geolocation";
    }
}