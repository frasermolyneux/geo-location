using MX.GeoLocation.Abstractions.Models.V1_1;

using MX.Api.Abstractions;

namespace MX.GeoLocation.Abstractions.Interfaces.V1_1
{
    /// <summary>
    /// Interface for GeoLookup API version 1.1 operations.
    /// Provides separate city and insights endpoints with typed response models.
    /// </summary>
    public interface IGeoLookupApi
    {
        /// <summary>
        /// Gets city-level geolocation information for a hostname or IP address using MaxMind City lookup.
        /// </summary>
        Task<ApiResult<CityGeoLocationDto>> GetCityGeoLocation(string hostname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets insights-level geolocation information for a hostname or IP address using MaxMind Insights lookup.
        /// Includes anonymizer data (VPN, proxy, Tor detection) in addition to city data.
        /// </summary>
        Task<ApiResult<InsightsGeoLocationDto>> GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets ProxyCheck.io risk assessment data for a hostname or IP address.
        /// </summary>
        Task<ApiResult<ProxyCheckDto>> GetProxyCheck(string hostname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets aggregated IP intelligence combining MaxMind Insights and ProxyCheck data.
        /// Returns source status metadata indicating which providers contributed data.
        /// </summary>
        Task<ApiResult<IpIntelligenceDto>> GetIpIntelligence(string hostname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets aggregated IP intelligence for multiple hostnames or IP addresses.
        /// </summary>
        Task<ApiResult<CollectionModel<IpIntelligenceDto>>> GetIpIntelligences(List<string> hostnames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all cached data for a hostname or IP address across all v1.1 cache tables.
        /// </summary>
        Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default);
    }
}
