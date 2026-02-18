using System.Net;
using System.Net.Sockets;

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
        public async Task<IActionResult> GetCityGeoLocationAction(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                var badRequestResponse = new ApiResponse<CityGeoLocationDto>(
                    new ApiError(ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME));
                return badRequestResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
            }

            var response = await ((Abstractions.Interfaces.V1_1.IGeoLookupApi)this).GetCityGeoLocation(hostname, cancellationToken);
            return response.ToHttpResult();
        }

        async Task<ApiResult<CityGeoLocationDto>> Abstractions.Interfaces.V1_1.IGeoLookupApi.GetCityGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            return await ExecuteLookup(hostname, cancellationToken, async address =>
            {
                // Try cache first
                var cached = await tableStorageGeoLocationRepository.GetCityGeoLocation(address, cancellationToken);
                if (cached is not null)
                {
                    _logger.LogInformation("Found cached city geolocation data for {Address}", address);
                    cached.Address = hostname;
                    return new ApiResponse<CityGeoLocationDto>(cached).ToApiResult();
                }

                // Fallback to MaxMind
                _logger.LogInformation("No cached data found, querying MaxMind city for {Address}", address);
                var result = await maxMindGeoLocationRepository.GetCityGeoLocation(address, cancellationToken);
                result.Address = hostname;

                await tableStorageGeoLocationRepository.StoreCityGeoLocation(result, cancellationToken);
                _logger.LogInformation("Stored city geolocation data for {Address}", address);

                return new ApiResponse<CityGeoLocationDto>(result).ToApiResult();
            });
        }

        [HttpGet]
        [Route("lookup/insights/{hostname}")]
        public async Task<IActionResult> GetInsightsGeoLocationAction(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                var badRequestResponse = new ApiResponse<InsightsGeoLocationDto>(
                    new ApiError(ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME));
                return badRequestResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
            }

            var response = await ((Abstractions.Interfaces.V1_1.IGeoLookupApi)this).GetInsightsGeoLocation(hostname, cancellationToken);
            return response.ToHttpResult();
        }

        async Task<ApiResult<InsightsGeoLocationDto>> Abstractions.Interfaces.V1_1.IGeoLookupApi.GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            return await ExecuteLookup(hostname, cancellationToken, async address =>
            {
                // Try cache first (with TTL check)
                var cached = await tableStorageGeoLocationRepository.GetInsightsGeoLocation(address, insightsCacheDuration, cancellationToken);
                if (cached is not null)
                {
                    _logger.LogInformation("Found cached insights geolocation data for {Address}", address);
                    cached.Address = hostname;
                    return new ApiResponse<InsightsGeoLocationDto>(cached).ToApiResult();
                }

                // Fallback to MaxMind
                _logger.LogInformation("No cached data found, querying MaxMind insights for {Address}", address);
                var result = await maxMindGeoLocationRepository.GetInsightsGeoLocation(address, cancellationToken);
                result.Address = hostname;

                await tableStorageGeoLocationRepository.StoreInsightsGeoLocation(result, cancellationToken);
                _logger.LogInformation("Stored insights geolocation data for {Address}", address);

                return new ApiResponse<InsightsGeoLocationDto>(result).ToApiResult();
            });
        }

        private async Task<ApiResult<T>> ExecuteLookup<T>(string hostname, CancellationToken cancellationToken, Func<string, Task<ApiResult<T>>> lookupFunc) where T : class
        {
            try
            {
                var (convertSuccess, validatedAddress) = await ConvertHostname(hostname, cancellationToken);
                if (convertSuccess && validatedAddress is not null)
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

        private async Task<(bool Success, string? ValidatedAddress)> ConvertHostname(string address, CancellationToken cancellationToken)
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                return (true, ipAddress.ToString());
            }

            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(address, cancellationToken);
                if (hostEntry.AddressList.FirstOrDefault() is not null)
                {
                    return (true, hostEntry.AddressList.First().ToString());
                }
            }
            catch (SocketException)
            {
                return (false, null);
            }

            return (false, null);
        }
    }
}
