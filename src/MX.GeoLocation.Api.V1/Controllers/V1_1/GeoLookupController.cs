using System.Net;

using MaxMind.GeoIP2.Exceptions;

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
        private readonly ILogger<GeoLookupController> _logger;
        private readonly IMaxMindGeoLocationRepository _maxMind;
        private readonly ITableStorageGeoLocationRepository _tableStorage;
        private readonly IHostnameResolver _hostnameResolver;
        private readonly TimeSpan _insightsCacheDuration;

        public GeoLookupController(
            ILogger<GeoLookupController> logger,
            IMaxMindGeoLocationRepository maxMind,
            ITableStorageGeoLocationRepository tableStorage,
            IHostnameResolver hostnameResolver,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxMind = maxMind ?? throw new ArgumentNullException(nameof(maxMind));
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _hostnameResolver = hostnameResolver ?? throw new ArgumentNullException(nameof(hostnameResolver));

            var days = configuration.GetValue("Caching:InsightsCacheDays", 7);
            _insightsCacheDuration = TimeSpan.FromDays(days);
        }

        [HttpGet]
        [Route("lookup/city/{hostname}")]
        public async Task<IActionResult> GetCityGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return ErrorResult<CityGeoLocationDto>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME);

            var response = await ExecuteLookup<CityGeoLocationDto>(hostname, cancellationToken, async address =>
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

            var response = await ExecuteLookup<InsightsGeoLocationDto>(hostname, cancellationToken, async address =>
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

        private async Task<ApiResult<T>> ExecuteLookup<T>(string hostname, CancellationToken cancellationToken, Func<string, Task<ApiResult<T>>> lookupFunc) where T : class
        {
            try
            {
                var (success, address) = await _hostnameResolver.ResolveHostname(hostname, cancellationToken);
                if (!success || address is null)
                    return new ApiResponse<T>(new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME)).ToApiResult(HttpStatusCode.BadRequest);

                if (_hostnameResolver.IsLocalAddress(hostname))
                    return new ApiResponse<T>(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS)).ToApiResult(HttpStatusCode.BadRequest);

                return await lookupFunc(address);
            }
            catch (AddressNotFoundException ex)
            {
                _logger.LogWarning(ex, "Address not found for {Hostname}", hostname);
                return new ApiResponse<T>(new ApiError(ErrorCodes.ADDRESS_NOT_FOUND, ErrorMessages.ADDRESS_NOT_FOUND)).ToApiResult(HttpStatusCode.NotFound);
            }
            catch (GeoIP2Exception ex)
            {
                _logger.LogError(ex, "GeoIP2 error for {Hostname}", hostname);
                return new ApiResponse<T>(new ApiError(ErrorCodes.GEOIP_ERROR, ex.Message)).ToApiResult(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during geolocation lookup for {Hostname}", hostname);
                return new ApiResponse<T>(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred")).ToApiResult(HttpStatusCode.InternalServerError);
            }
        }

        private static IActionResult ErrorResult<T>(HttpStatusCode statusCode, string code, string message) where T : class
        {
            return new ApiResponse<T>(new ApiError(code, message)).ToApiResult(statusCode).ToHttpResult();
        }
    }
}
