using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MX.GeoLocation.Abstractions.Models.V1_1
{
    /// <summary>
    /// Aggregated IP intelligence combining MaxMind Insights geolocation/anonymizer data
    /// with ProxyCheck.io risk assessment. Source status metadata indicates which providers
    /// contributed data, enabling consumers to handle partial results.
    /// </summary>
    public record IpIntelligenceDto
    {
        // Identity

        [JsonProperty]
        public string Address { get; internal set; } = string.Empty;

        [JsonProperty]
        public string TranslatedAddress { get; internal set; } = string.Empty;

        // Geo context (from MaxMind Insights)

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
        public List<string> Subdivisions { get; internal set; } = [];

        [JsonProperty]
        public double? Latitude { get; internal set; }

        [JsonProperty]
        public double? Longitude { get; internal set; }

        [JsonProperty]
        public int? AccuracyRadius { get; internal set; }

        [JsonProperty]
        public string? Timezone { get; internal set; }

        // Network (from MaxMind NetworkTraits)

        [JsonProperty]
        public NetworkTraitsDto? NetworkTraits { get; internal set; }

        // Anonymizer (from MaxMind Insights)

        [JsonProperty]
        public AnonymizerDto? Anonymizer { get; internal set; }

        // Risk (from ProxyCheck — nested to avoid field ambiguity with MaxMind geo data)

        [JsonProperty]
        public ProxyCheckDto? ProxyCheck { get; internal set; }

        // Source status metadata

        /// <summary>Status of the MaxMind Insights data source for this lookup.</summary>
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public SourceStatus MaxMindStatus { get; internal set; }

        /// <summary>Status of the ProxyCheck.io data source for this lookup.</summary>
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public SourceStatus ProxyCheckStatus { get; internal set; }

        /// <summary>True if either data source failed, indicating a partial result.</summary>
        [JsonProperty]
        public bool IsPartial { get; internal set; }
    }
}
