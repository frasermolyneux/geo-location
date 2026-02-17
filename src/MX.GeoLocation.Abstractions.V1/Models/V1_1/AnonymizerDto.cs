using Newtonsoft.Json;

namespace MX.GeoLocation.Abstractions.Models.V1_1
{
    /// <summary>
    /// Anonymizer data from MaxMind Insights responses.
    /// Contains information about whether an IP is associated with VPNs,
    /// proxies, hosting providers, or Tor exit nodes.
    /// </summary>
    public record AnonymizerDto
    {
        [JsonProperty]
        public int? Confidence { get; internal set; }

        [JsonProperty]
        public bool IsAnonymous { get; internal set; }

        [JsonProperty]
        public bool IsAnonymousVpn { get; internal set; }

        [JsonProperty]
        public bool IsHostingProvider { get; internal set; }

        [JsonProperty]
        public bool IsPublicProxy { get; internal set; }

        [JsonProperty]
        public bool IsResidentialProxy { get; internal set; }

        [JsonProperty]
        public bool IsTorExitNode { get; internal set; }

        [JsonProperty]
        public string? NetworkLastSeen { get; internal set; }

        [JsonProperty]
        public string? ProviderName { get; internal set; }
    }
}
