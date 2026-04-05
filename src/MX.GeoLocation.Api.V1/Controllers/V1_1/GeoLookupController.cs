using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using MX.Api.Abstractions;
using MX.Api.Web.Extensions;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Constants;
using MX.GeoLocation.LookupWebApi.Repositories;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.LookupWebApi.Controllers.V1_1
{
    [ApiController]
    [ApiVersion("1.1")]
    [Route("v{version:apiVersion}")]
    [Authorize(Roles = "LookupApiUser")]
    public class GeoLookupController : ControllerBase
    {
        private const int MaxBatchSize = 20;

        private readonly IMaxMindGeoLocationRepository _maxMind;
        private readonly ITableStorageGeoLocationRepository _tableStorage;
        private readonly IProxyCheckRepository _proxyCheck;
        private readonly IProxyCheckCacheRepository _proxyCheckCache;
        private readonly IGeoLookupService _geoLookupService;
        private readonly IIpIntelligenceService _intelligenceService;
        private readonly IHostnameResolver _hostnameResolver;
        private readonly ILogger<GeoLookupController> _logger;
        private readonly TimeSpan _insightsCacheDuration;
        private readonly TimeSpan _proxyCheckCacheDuration;

        public GeoLookupController(
            IMaxMindGeoLocationRepository maxMind,
            ITableStorageGeoLocationRepository tableStorage,
            IProxyCheckRepository proxyCheck,
            IProxyCheckCacheRepository proxyCheckCache,
            IGeoLookupService geoLookupService,
            IIpIntelligenceService intelligenceService,
            IHostnameResolver hostnameResolver,
            IConfiguration configuration,
            ILogger<GeoLookupController> logger)
        {
            _maxMind = maxMind ?? throw new ArgumentNullException(nameof(maxMind));
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _proxyCheck = proxyCheck ?? throw new ArgumentNullException(nameof(proxyCheck));
            _proxyCheckCache = proxyCheckCache ?? throw new ArgumentNullException(nameof(proxyCheckCache));
            _geoLookupService = geoLookupService ?? throw new ArgumentNullException(nameof(geoLookupService));
            _intelligenceService = intelligenceService ?? throw new ArgumentNullException(nameof(intelligenceService));
            _hostnameResolver = hostnameResolver ?? throw new ArgumentNullException(nameof(hostnameResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _insightsCacheDuration = TimeSpan.FromDays(configuration.GetValue("Caching:InsightsCacheDays", 7));
            _proxyCheckCacheDuration = TimeSpan.FromMinutes(configuration.GetValue("Caching:ProxyCheckCacheMinutes", 60));
        }

        [HttpGet]
        [Route("lookup/city/{hostname}")]
        public async Task<IActionResult> GetCityGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return ErrorResult<CityGeoLocationDto>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME);

            var response = await _geoLookupService.ExecuteLookup<CityGeoLocationDto>(hostname, cancellationToken, async address =>
            {
                var cached = await _tableStorage.GetCityGeoLocation(address, cancellationToken);
                if (cached is not null)
                {
                    cached.Address = hostname;
                    return new ApiResponse<CityGeoLocationDto>(cached).ToApiResult();
                }

                var result = await _maxMind.GetCityGeoLocation(address, cancellationToken);
                result.Address = hostname;
                await _tableStorage.StoreCityGeoLocation(result, cancellationToken);

                return new ApiResponse<CityGeoLocationDto>(result).ToApiResult();
            });

            return response.ToHttpResult();
        }

        [HttpGet]
        [Route("lookup/insights/{hostname}")]
        public async Task<IActionResult> GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return ErrorResult<InsightsGeoLocationDto>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME);

            var response = await _geoLookupService.ExecuteLookup<InsightsGeoLocationDto>(hostname, cancellationToken, async address =>
            {
                var cached = await _tableStorage.GetInsightsGeoLocation(address, _insightsCacheDuration, cancellationToken);
                if (cached is not null)
                {
                    cached.Address = hostname;
                    return new ApiResponse<InsightsGeoLocationDto>(cached).ToApiResult();
                }

                var result = await _maxMind.GetInsightsGeoLocation(address, cancellationToken);
                result.Address = hostname;
                await _tableStorage.StoreInsightsGeoLocation(result, cancellationToken);

                return new ApiResponse<InsightsGeoLocationDto>(result).ToApiResult();
            });

            return response.ToHttpResult();
        }

        [HttpGet]
        [Route("lookup/proxycheck/{hostname}")]
        public async Task<IActionResult> GetProxyCheck(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return ErrorResult<ProxyCheckDto>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME);

            var response = await _geoLookupService.ExecuteLookup<ProxyCheckDto>(hostname, cancellationToken, async address =>
            {
                var cached = await _proxyCheckCache.GetProxyCheckData(address, _proxyCheckCacheDuration, cancellationToken);
                if (cached is not null)
                {
                    cached.Address = hostname;
                    return new ApiResponse<ProxyCheckDto>(cached).ToApiResult();
                }

                var result = await _proxyCheck.GetProxyCheckData(address, cancellationToken);
                result.Address = hostname;
                await _proxyCheckCache.StoreProxyCheckData(result, cancellationToken);

                return new ApiResponse<ProxyCheckDto>(result).ToApiResult();
            });

            return response.ToHttpResult();
        }

        [HttpGet]
        [Route("lookup/intelligence/{hostname}")]
        public async Task<IActionResult> GetIpIntelligence(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return ErrorResult<IpIntelligenceDto>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME);

            var response = await _geoLookupService.ExecuteLookup<IpIntelligenceDto>(hostname, cancellationToken, async address =>
            {
                var result = await _intelligenceService.GetIpIntelligence(hostname, address, cancellationToken);

                if (result is null)
                    return new ApiResponse<IpIntelligenceDto>(new ApiError(ErrorCodes.INTERNAL_ERROR, "All data sources failed"))
                        .ToApiResult(HttpStatusCode.ServiceUnavailable);

                return new ApiResponse<IpIntelligenceDto>(result).ToApiResult();
            });

            return response.ToHttpResult();
        }

        [HttpPost]
        [Route("lookup/intelligence")]
        public async Task<IActionResult> GetIpIntelligences([FromBody] List<string>? hostnames, CancellationToken cancellationToken)
        {
            if (hostnames is null)
                return ErrorResult<CollectionModel<IpIntelligenceDto>>(HttpStatusCode.BadRequest, ErrorCodes.NULL_REQUEST, ErrorMessages.NULL_REQUEST);

            if (hostnames.Count == 0)
                return ErrorResult<CollectionModel<IpIntelligenceDto>>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_REQUEST_LIST, ErrorMessages.EMPTY_REQUEST_LIST);

            if (hostnames.Count > MaxBatchSize)
                return ErrorResult<CollectionModel<IpIntelligenceDto>>(HttpStatusCode.BadRequest, ErrorCodes.INVALID_HOSTNAME, $"Batch requests are limited to {MaxBatchSize} hostnames.");

            List<IpIntelligenceDto> entries = [];
            List<ApiError> errors = [];

            await Parallel.ForEachAsync(
                hostnames,
                new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
                async (hostname, ct) =>
            {
                var lookupResult = await _geoLookupService.ExecuteLookup<IpIntelligenceDto>(hostname, ct, async address =>
                {
                    var result = await _intelligenceService.GetIpIntelligence(hostname, address, ct);

                    if (result is null)
                        return new ApiResponse<IpIntelligenceDto>(new ApiError(ErrorCodes.INTERNAL_ERROR, "All data sources failed"))
                            .ToApiResult(HttpStatusCode.ServiceUnavailable);

                    return new ApiResponse<IpIntelligenceDto>(result).ToApiResult();
                });

                if (lookupResult.StatusCode == HttpStatusCode.OK && lookupResult.Result?.Data is not null)
                {
                    lock (entries) entries.Add(lookupResult.Result.Data);
                }
                else if (lookupResult.Result?.Errors is not null)
                {
                    lock (errors) errors.AddRange(lookupResult.Result.Errors);
                }
            });

            var result = new ApiResponse<CollectionModel<IpIntelligenceDto>>(
                new CollectionModel<IpIntelligenceDto> { Items = entries })
            {
                Errors = [.. errors],
                Pagination = new ApiPagination(entries.Count, entries.Count, 0, 0)
            }.ToApiResult();

            return result.ToHttpResult();
        }

        [HttpDelete]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> DeleteMetadata(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return new ApiResponse(new ApiError(ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME)).ToBadRequestResult().ToHttpResult();

            try
            {
                var (success, address) = await _hostnameResolver.ResolveHostname(hostname, cancellationToken);
                if (!success || address is null)
                    return new ApiResponse(new ApiError(ErrorCodes.HOSTNAME_RESOLUTION_FAILED, ErrorMessages.HOSTNAME_RESOLUTION_FAILED_DELETE)).ToBadRequestResult().ToHttpResult();

                if (_hostnameResolver.IsLocalAddress(hostname) || _hostnameResolver.IsPrivateOrReservedAddress(address))
                    return new ApiResponse(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS_DELETE)).ToBadRequestResult().ToHttpResult();

                var deleted = false;

                // Delete from all cache tables (v1.0 + v1.1 + proxycheck)
                deleted |= await _tableStorage.DeleteGeoLocation(address, cancellationToken);
                if (!string.Equals(hostname, address, StringComparison.OrdinalIgnoreCase))
                    deleted |= await _tableStorage.DeleteGeoLocation(hostname, cancellationToken);

                deleted |= await _proxyCheckCache.DeleteProxyCheckData(address, cancellationToken);

                return deleted
                    ? new ApiResponse().ToApiResult().ToHttpResult()
                    : new ApiResponse(new ApiError(ErrorCodes.NOT_FOUND, ErrorMessages.NOT_FOUND)).ToNotFoundResult().ToHttpResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cached data for {Hostname}", hostname);
                return new ApiResponse(new ApiError(ErrorCodes.INTERNAL_ERROR, "An error occurred while deleting data"))
                    .ToApiResult(HttpStatusCode.InternalServerError).ToHttpResult();
            }
        }

        private static IActionResult ErrorResult<T>(HttpStatusCode statusCode, string code, string message) where T : class
        {
            return new ApiResponse<T>(new ApiError(code, message)).ToApiResult(statusCode).ToHttpResult();
        }
    }
}
