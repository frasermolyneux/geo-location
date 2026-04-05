using Newtonsoft.Json;

namespace MX.GeoLocation.Abstractions.Models.V1_1
{
    /// <summary>
    /// IP risk assessment data from ProxyCheck.io.
    /// Contains risk score, proxy/VPN detection, and network information.
    /// </summary>
    public record ProxyCheckDto
    {
        [JsonProperty]
        public string Address { get; internal set; } = string.Empty;

        [JsonProperty]
        public string TranslatedAddress { get; internal set; } = string.Empty;

        /// <summary>Risk score from 0-100, with higher scores indicating higher risk.</summary>
        [JsonProperty]
        public int RiskScore { get; internal set; }

        /// <summary>Indicates if the IP address is identified as a proxy.</summary>
        [JsonProperty]
        public bool IsProxy { get; internal set; }

        /// <summary>Indicates if the IP address is identified as a VPN.</summary>
        [JsonProperty]
        public bool IsVpn { get; internal set; }

        /// <summary>The type of connection (VPN, TOR, PROXY, DCH, etc.).</summary>
        [JsonProperty]
        public string ProxyType { get; internal set; } = string.Empty;

        /// <summary>Country where the IP address is located (from ProxyCheck).</summary>
        [JsonProperty]
        public string Country { get; internal set; } = string.Empty;

        /// <summary>Region/state where the IP address is located (from ProxyCheck).</summary>
        [JsonProperty]
        public string Region { get; internal set; } = string.Empty;

        /// <summary>Autonomous System Number associated with the IP address.</summary>
        [JsonProperty]
        public string AsNumber { get; internal set; } = string.Empty;

        /// <summary>Organization that owns the Autonomous System.</summary>
        [JsonProperty]
        public string AsOrganization { get; internal set; } = string.Empty;
    }
}
