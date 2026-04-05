using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Services
{
    public interface IIpIntelligenceService
    {
        /// <summary>
        /// Gets aggregated IP intelligence by looking up MaxMind Insights and ProxyCheck in parallel.
        /// Returns partial results with source status metadata if one source fails.
        /// Returns null if both sources fail.
        /// </summary>
        Task<IpIntelligenceDto?> GetIpIntelligence(string hostname, string resolvedAddress, CancellationToken cancellationToken = default);
    }
}
