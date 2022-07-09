using MaxMind.GeoIP2;

using MX.GeoLocation.LookupApi.Abstractions.Models;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public class MaxMindGeoLocationRepository : IMaxMindGeoLocationRepository
    {
        private readonly IConfiguration configuration;

        public MaxMindGeoLocationRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<GeoLocationDto> GetGeoLocation(string address)
        {
            var userId = Convert.ToInt32(configuration["maxmind_userid"]);

            using (var reader = new WebServiceClient(userId, configuration["maxmind_apikey"]))
            {
                var lookupResult = await reader.CityAsync(address);

                var traits = new Dictionary<string, string?>
                {
                    {"AutonomousSystemNumber", lookupResult.Traits.AutonomousSystemNumber?.ToString()},
                    {"AutonomousSystemOrganization", lookupResult.Traits?.AutonomousSystemOrganization},
                    {"ConnectionType", lookupResult.Traits?.ConnectionType},
                    {"Domain", lookupResult.Traits?.Domain},
                    {"IPAddress", lookupResult.Traits?.IPAddress},
                    {"IsAnonymous", lookupResult.Traits?.IsAnonymous.ToString()},
                    {"IsAnonymousVpn", lookupResult.Traits?.IsAnonymousVpn.ToString()},
                    {"IsHostingProvider", lookupResult.Traits?.IsHostingProvider.ToString()},
                    {"IsLegitimateProxy", lookupResult.Traits?.IsLegitimateProxy.ToString()},
                    {"IsPublicProxy", lookupResult.Traits?.IsPublicProxy.ToString()},
                    {"IsTorExitNode", lookupResult.Traits?.IsTorExitNode.ToString()},
                    {"Isp", lookupResult.Traits?.Isp},
                    {"Organization", lookupResult.Traits?.Organization},
                    {"StaticIPScore", lookupResult.Traits?.StaticIPScore.ToString()},
                    {"UserCount", lookupResult.Traits?.UserCount.ToString()},
                    {"UserType", lookupResult.Traits?.UserType}
                };

                var geoLocationDto =
                    new GeoLocationDto()
                    {
                        Address = address,
                        TranslatedAddress = address,
                        ContinentCode = lookupResult.Continent?.Code ?? string.Empty,
                        ContinentName = lookupResult.Continent?.Name ?? string.Empty,
                        CountryCode = lookupResult.Country?.IsoCode ?? string.Empty,
                        CountryName = lookupResult.Country?.Name ?? string.Empty,
                        IsEuropeanUnion = lookupResult.Country?.IsInEuropeanUnion ?? false,
                        CityName = lookupResult.City?.Name ?? string.Empty,
                        PostalCode = lookupResult.Postal?.Code ?? string.Empty,
                        RegisteredCountry = lookupResult.RegisteredCountry?.IsoCode ?? string.Empty,
                        RepresentedCountry = lookupResult.RepresentedCountry?.IsoCode ?? string.Empty,
                        Latitude = lookupResult.Location?.Latitude ?? 0.0,
                        Longitude = lookupResult.Location?.Longitude ?? 0.0,
                        AccuracyRadius = lookupResult.Location?.AccuracyRadius ?? 0,
                        Timezone = lookupResult.Location?.TimeZone ?? string.Empty,
                        Traits = traits
                    };

                return geoLocationDto;
            }
        }
    }
}
