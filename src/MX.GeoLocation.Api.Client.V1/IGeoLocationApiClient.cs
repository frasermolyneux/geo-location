using MX.GeoLocation.LookupApi.Abstractions.Interfaces;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public interface IGeoLocationApiClient
    {
        public IGeoLookupApi GeoLookup { get; }
    }
}