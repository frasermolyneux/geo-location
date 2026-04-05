using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Repositories;

namespace MX.GeoLocation.LookupWebApi.Services
{
    public class IpIntelligenceService : IIpIntelligenceService
    {
        private readonly IMaxMindGeoLocationRepository _maxMind;
        private readonly ITableStorageGeoLocationRepository _geoTableStorage;
        private readonly IProxyCheckRepository _proxyCheck;
        private readonly IProxyCheckCacheRepository _proxyCheckCache;
        private readonly ILogger<IpIntelligenceService> _logger;
        private readonly TimeSpan _insightsCacheDuration;
        private readonly TimeSpan _proxyCheckCacheDuration;

        public IpIntelligenceService(
            IMaxMindGeoLocationRepository maxMind,
            ITableStorageGeoLocationRepository geoTableStorage,
            IProxyCheckRepository proxyCheck,
            IProxyCheckCacheRepository proxyCheckCache,
            IConfiguration configuration,
            ILogger<IpIntelligenceService> logger)
        {
            _maxMind = maxMind ?? throw new ArgumentNullException(nameof(maxMind));
            _geoTableStorage = geoTableStorage ?? throw new ArgumentNullException(nameof(geoTableStorage));
            _proxyCheck = proxyCheck ?? throw new ArgumentNullException(nameof(proxyCheck));
            _proxyCheckCache = proxyCheckCache ?? throw new ArgumentNullException(nameof(proxyCheckCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _insightsCacheDuration = TimeSpan.FromDays(configuration.GetValue("Caching:InsightsCacheDays", 7));
            _proxyCheckCacheDuration = TimeSpan.FromMinutes(configuration.GetValue("Caching:ProxyCheckCacheMinutes", 60));
        }

        public async Task<IpIntelligenceDto?> GetIpIntelligence(string hostname, string resolvedAddress, CancellationToken cancellationToken = default)
        {
            // Fan out both lookups in parallel
            var insightsTask = GetInsightsData(resolvedAddress, cancellationToken);
            var proxyCheckTask = GetProxyCheckData(resolvedAddress, cancellationToken);

            await Task.WhenAll(insightsTask, proxyCheckTask).ConfigureAwait(false);

            var (insights, maxMindStatus) = await insightsTask;
            var (proxyCheckDto, proxyCheckStatus) = await proxyCheckTask;

            // Both failed — return null to signal error
            if (maxMindStatus == SourceStatus.Failed && proxyCheckStatus == SourceStatus.Failed)
            {
                _logger.LogError("Both MaxMind and ProxyCheck failed for {Address}", resolvedAddress);
                return null;
            }

            var dto = new IpIntelligenceDto
            {
                Address = hostname,
                TranslatedAddress = resolvedAddress,
                MaxMindStatus = maxMindStatus,
                ProxyCheckStatus = proxyCheckStatus,
                IsPartial = maxMindStatus != SourceStatus.Success || proxyCheckStatus != SourceStatus.Success
            };

            // Populate MaxMind fields if available
            if (insights is not null)
            {
                dto.ContinentCode = insights.ContinentCode;
                dto.ContinentName = insights.ContinentName;
                dto.CountryCode = insights.CountryCode;
                dto.CountryName = insights.CountryName;
                dto.IsEuropeanUnion = insights.IsEuropeanUnion;
                dto.CityName = insights.CityName;
                dto.PostalCode = insights.PostalCode;
                dto.Subdivisions = insights.Subdivisions;
                dto.Latitude = insights.Latitude;
                dto.Longitude = insights.Longitude;
                dto.AccuracyRadius = insights.AccuracyRadius;
                dto.Timezone = insights.Timezone;
                dto.NetworkTraits = insights.NetworkTraits;
                dto.Anonymizer = insights.Anonymizer;
            }

            // Nest ProxyCheck data if available
            if (proxyCheckDto is not null)
            {
                dto.ProxyCheck = proxyCheckDto;
            }

            return dto;
        }

        private async Task<(InsightsGeoLocationDto?, SourceStatus)> GetInsightsData(string address, CancellationToken cancellationToken)
        {
            try
            {
                // Cache-first
                var cached = await _geoTableStorage.GetInsightsGeoLocation(address, _insightsCacheDuration, cancellationToken).ConfigureAwait(false);
                if (cached is not null)
                    return (cached, SourceStatus.Success);

                var result = await _maxMind.GetInsightsGeoLocation(address, cancellationToken).ConfigureAwait(false);
                await _geoTableStorage.StoreInsightsGeoLocation(result, cancellationToken).ConfigureAwait(false);
                return (result, SourceStatus.Success);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "MaxMind Insights lookup failed for {Address}", address);
                return (null, SourceStatus.Failed);
            }
        }

        private async Task<(ProxyCheckDto?, SourceStatus)> GetProxyCheckData(string address, CancellationToken cancellationToken)
        {
            try
            {
                // Cache-first
                var cached = await _proxyCheckCache.GetProxyCheckData(address, _proxyCheckCacheDuration, cancellationToken).ConfigureAwait(false);
                if (cached is not null)
                    return (cached, SourceStatus.Success);

                var result = await _proxyCheck.GetProxyCheckData(address, cancellationToken).ConfigureAwait(false);

                // Only cache successful responses
                await _proxyCheckCache.StoreProxyCheckData(result, cancellationToken).ConfigureAwait(false);
                return (result, SourceStatus.Success);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ProxyCheck lookup failed for {Address}", address);
                return (null, SourceStatus.Failed);
            }
        }
    }
}
