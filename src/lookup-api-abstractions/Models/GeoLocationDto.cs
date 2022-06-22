namespace MX.GeoLocation.LookupApi.Abstractions.Models
{
    public class GeoLocationDto
    {
        public string? Address { get; internal set; }
        public string? TranslatedAddress { get; internal set; }

        public string? ContinentCode { get; internal set; }
        public string? ContinentName { get; internal set; }
        public string? CountryCode { get; internal set; }
        public string? CountryName { get; internal set; }
        public bool IsEuropeanUnion { get; internal set; }
        public string? CityName { get; internal set; }
        public string? PostalCode { get; internal set; }
        public string? RegisteredCountry { get; internal set; }
        public string? RepresentedCountry { get; internal set; }
        public double? Latitude { get; internal set; }
        public double? Longitude { get; internal set; }
        public int? AccuracyRadius { get; internal set; }
        public string? Timezone { get; internal set; }
        public Dictionary<string, string> Traits { get; set; } = new Dictionary<string, string>();
    }
}
