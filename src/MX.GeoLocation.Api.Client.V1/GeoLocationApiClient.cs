using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Api.Client.V1
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