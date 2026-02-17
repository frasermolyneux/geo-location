using Newtonsoft.Json;

namespace MX.GeoLocation.Abstractions.Models.V1_1
{
    /// <summary>
    /// Network and ISP traits for an IP address from MaxMind City/Insights responses.
    /// </summary>
    public record NetworkTraitsDto
    {
        [JsonProperty]
        public long? AutonomousSystemNumber { get; internal set; }

        [JsonProperty]
        public string? AutonomousSystemOrganization { get; internal set; }

        [JsonProperty]
        public string? ConnectionType { get; internal set; }

        [JsonProperty]
        public string? Domain { get; internal set; }

        [JsonProperty]
        public string? IPAddress { get; internal set; }

        [JsonProperty]
        public bool IsAnycast { get; internal set; }

        [JsonProperty]
        public string? Isp { get; internal set; }

        [JsonProperty]
        public string? MobileCountryCode { get; internal set; }

        [JsonProperty]
        public string? MobileNetworkCode { get; internal set; }

        [JsonProperty]
        public string? Network { get; internal set; }

        [JsonProperty]
        public string? Organization { get; internal set; }

        [JsonProperty]
        public double? StaticIPScore { get; internal set; }

        [JsonProperty]
        public int? UserCount { get; internal set; }

        [JsonProperty]
        public string? UserType { get; internal set; }
    }
}
