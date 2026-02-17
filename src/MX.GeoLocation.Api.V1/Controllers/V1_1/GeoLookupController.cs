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

namespace MX.GeoLocation.LookupWebApi.Controllers.V1_1
{
    [ApiController]
    [ApiVersion("1.1")]
    [Route("v{version:apiVersion}")]
    [Authorize(Roles = "LookupApiUser")]
    public class GeoLookupController : Controller, Abstractions.Interfaces.V1_1.IGeoLookupApi
    {
        private readonly ILogger<GeoLookupController> _logger;
        private readonly IMaxMindGeoLocationRepository maxMindGeoLocationRepository;
        private readonly ITableStorageGeoLocationRepository tableStorageGeoLocationRepository;
        private readonly TimeSpan insightsCacheDuration;

        private readonly string[] localOverrides = { "localhost", "127.0.0.1" };

        public GeoLookupController(
            ILogger<GeoLookupController> logger,
            IMaxMindGeoLocationRepository maxMindGeoLocationRepository,
            ITableStorageGeoLocationRepository tableStorageGeoLocationRepository,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.maxMindGeoLocationRepository = maxMindGeoLocationRepository ?? throw new ArgumentNullException(nameof(maxMindGeoLocationRepository));
            this.tableStorageGeoLocationRepository = tableStorageGeoLocationRepository ?? throw new ArgumentNullException(nameof(tableStorageGeoLocationRepository));

            var days = configuration.GetValue("Caching:InsightsCacheDays", 7);
            insightsCacheDuration = TimeSpan.FromDays(days);
        }

        [HttpGet]
        [Route("lookup/city/{hostname}")]
        public async Task<IActionResult> GetCityGeoLocationAction(string hostname)
        {
            var response = await ((Abstractions.Interfaces.V1_1.IGeoLookupApi)this).GetCityGeoLocation(hostname, default);
            return response.ToHttpResult();
        }

        async Task<ApiResult<CityGeoLocationDto>> Abstractions.Interfaces.V1_1.IGeoLookupApi.GetCityGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            return await ExecuteLookup(hostname, async address =>
            {
                // Try cache first
                var cached = await tableStorageGeoLocationRepository.GetCityGeoLocation(address);
                if (cached is not null)
                {
                    _logger.LogInformation("Found cached city geolocation data for {Address}", address);
                    cached.Address = hostname;
                    return new ApiResponse<CityGeoLocationDto>(cached).ToApiResult();
                }

                // Fallback to MaxMind
                _logger.LogInformation("No cached data found, querying MaxMind city for {Address}", address);
                var result = await maxMindGeoLocationRepository.GetCityGeoLocation(address);
                result.Address = hostname;

                await tableStorageGeoLocationRepository.StoreCityGeoLocation(result);
                _logger.LogInformation("Stored city geolocation data for {Address}", address);

                return new ApiResponse<CityGeoLocationDto>(result).ToApiResult();
            });
        }

        [HttpGet]
        [Route("lookup/insights/{hostname}")]
        public async Task<IActionResult> GetInsightsGeoLocationAction(string hostname)
        {
            var response = await ((Abstractions.Interfaces.V1_1.IGeoLookupApi)this).GetInsightsGeoLocation(hostname, default);
            return response.ToHttpResult();
        }

        async Task<ApiResult<InsightsGeoLocationDto>> Abstractions.Interfaces.V1_1.IGeoLookupApi.GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            return await ExecuteLookup(hostname, async address =>
            {
                // Try cache first (with TTL check)
                var cached = await tableStorageGeoLocationRepository.GetInsightsGeoLocation(address, insightsCacheDuration);
                if (cached is not null)
                {
                    _logger.LogInformation("Found cached insights geolocation data for {Address}", address);
                    cached.Address = hostname;
                    return new ApiResponse<InsightsGeoLocationDto>(cached).ToApiResult();
                }

                // Fallback to MaxMind
                _logger.LogInformation("No cached data found, querying MaxMind insights for {Address}", address);
                var result = await maxMindGeoLocationRepository.GetInsightsGeoLocation(address);
                result.Address = hostname;

                await tableStorageGeoLocationRepository.StoreInsightsGeoLocation(result);
                _logger.LogInformation("Stored insights geolocation data for {Address}", address);

                return new ApiResponse<InsightsGeoLocationDto>(result).ToApiResult();
            });
        }

        private async Task<ApiResult<T>> ExecuteLookup<T>(string hostname, Func<string, Task<ApiResult<T>>> lookupFunc) where T : class
        {
            try
            {
                if (ConvertHostname(hostname, out var validatedAddress) && validatedAddress is not null)
                {
                    if (localOverrides.Contains(hostname))
                    {
                        return new ApiResponse<T>(
                            new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS))
                            .ToApiResult(HttpStatusCode.BadRequest);
                    }

                    return await lookupFunc(validatedAddress);
                }
                else
                {
                    // ConvertHostname fails for both invalid and unresolvable hostnames
                    return new ApiResponse<T>(
                        new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME))
                        .ToApiResult(HttpStatusCode.BadRequest);
                }
            }
            catch (AddressNotFoundException ex)
            {
                _logger.LogWarning(ex, "Address not found for {Hostname}", hostname);
                return new ApiResponse<T>(
                    new ApiError(ErrorCodes.ADDRESS_NOT_FOUND, ErrorMessages.ADDRESS_NOT_FOUND))
                    .ToApiResult(HttpStatusCode.NotFound);
            }
            catch (GeoIP2Exception ex)
            {
                _logger.LogError(ex, "GeoIP2 error for {Hostname}", hostname);
                return new ApiResponse<T>(
                    new ApiError(ErrorCodes.GEOIP_ERROR, ex.Message))
                    .ToApiResult(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during geolocation lookup for {Hostname}", hostname);
                return new ApiResponse<T>(
                    new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred"))
                    .ToApiResult(HttpStatusCode.InternalServerError);
            }
        }

        private bool ConvertHostname(string address, out string? validatedAddress)
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                validatedAddress = ipAddress.ToString();
                return true;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(address);
                if (hostEntry.AddressList.FirstOrDefault() is not null)
                {
                    validatedAddress = hostEntry.AddressList.First().ToString();
                    return true;
                }
            }
            catch
            {
                validatedAddress = null;
                return false;
            }

            validatedAddress = null;
            return false;
        }
    }
}
