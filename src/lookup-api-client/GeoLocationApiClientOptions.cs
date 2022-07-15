namespace MX.GeoLocation.GeoLocationApi.Client
{
    public class GeoLocationApiClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiPathPrefix { get; set; } = "geolocation";
    }
}