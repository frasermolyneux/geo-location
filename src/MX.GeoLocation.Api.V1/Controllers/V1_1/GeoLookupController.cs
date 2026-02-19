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
        private readonly IMaxMindGeoLocationRepository _maxMind;
        private readonly ITableStorageGeoLocationRepository _tableStorage;
        private readonly IGeoLookupService _geoLookupService;
        private readonly TimeSpan _insightsCacheDuration;

        public GeoLookupController(
            IMaxMindGeoLocationRepository maxMind,
            ITableStorageGeoLocationRepository tableStorage,
            IGeoLookupService geoLookupService,
            IConfiguration configuration)
        {
            _maxMind = maxMind ?? throw new ArgumentNullException(nameof(maxMind));
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _geoLookupService = geoLookupService ?? throw new ArgumentNullException(nameof(geoLookupService));

            var days = configuration.GetValue("Caching:InsightsCacheDays", 7);
            _insightsCacheDuration = TimeSpan.FromDays(days);
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

        private static IActionResult ErrorResult<T>(HttpStatusCode statusCode, string code, string message) where T : class
        {
            return new ApiResponse<T>(new ApiError(code, message)).ToApiResult(statusCode).ToHttpResult();
        }
    }
}
