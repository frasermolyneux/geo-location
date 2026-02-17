using Newtonsoft.Json;

namespace MX.GeoLocation.Abstractions.Models.V1_1
{
    /// <summary>
    /// GeoLocation result from MaxMind Insights lookup.
    /// Extends city data with the Anonymizer object.
    /// </summary>
    public record InsightsGeoLocationDto : CityGeoLocationDto
    {
        [JsonProperty]
        public AnonymizerDto Anonymizer { get; internal set; } = new();
    }
}
