using MX.GeoLocation.LookupApi.Abstractions.Interfaces;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public class GeoLocationApiClient : IGeoLocationApiClient
    {
        public GeoLocationApiClient(
            IGeoLookupApi geoLookupApi)
        {
            GeoLookup = geoLookupApi;
        }

        public IGeoLookupApi GeoLookup { get; }
    }
}