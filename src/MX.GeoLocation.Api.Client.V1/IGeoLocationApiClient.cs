using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Api.Client.V1
{
    public interface IGeoLocationApiClient
    {
        public IGeoLookupApi GeoLookup { get; }
    }
}