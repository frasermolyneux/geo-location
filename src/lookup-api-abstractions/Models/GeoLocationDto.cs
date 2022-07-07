using Newtonsoft.Json;

namespace MX.GeoLocation.LookupApi.Abstractions.Models
{
    public class GeoLocationDto
    {
        [JsonProperty]
        public string? Address { get; internal set; }

        [JsonProperty]
        public string? TranslatedAddress { get; internal set; }

        [JsonProperty]
        public string? ContinentCode { get; internal set; }

        [JsonProperty]
        public string? ContinentName { get; internal set; }

        [JsonProperty]
        public string? CountryCode { get; internal set; }

        [JsonProperty]
        public string? CountryName { get; internal set; }

        [JsonProperty]
        public bool IsEuropeanUnion { get; internal set; }

        [JsonProperty]
        public string? CityName { get; internal set; }

        [JsonProperty]
        public string? PostalCode { get; internal set; }

        [JsonProperty]
        public string? RegisteredCountry { get; internal set; }

        [JsonProperty]
        public string? RepresentedCountry { get; internal set; }

        [JsonProperty]
        public double? Latitude { get; internal set; }

        [JsonProperty]
        public double? Longitude { get; internal set; }

        [JsonProperty]
        public int? AccuracyRadius { get; internal set; }

        [JsonProperty]
        public string? Timezone { get; internal set; }

        [JsonProperty]
        public Dictionary<string, string?> Traits { get; internal set; } = new Dictionary<string, string?>();
    }
}
