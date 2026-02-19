using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using MX.Api.Abstractions;
using MX.Api.Web.Extensions;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Constants;
using MX.GeoLocation.LookupWebApi.Repositories;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.LookupWebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}")]
    [Authorize(Roles = "LookupApiUser")]
    public class GeoLookupController : ControllerBase
    {
        private const int MaxBatchSize = 20;

        private readonly ILogger<GeoLookupController> _logger;
        private readonly ITableStorageGeoLocationRepository _tableStorage;
        private readonly IMaxMindGeoLocationRepository _maxMind;
        private readonly IHostnameResolver _hostnameResolver;
        private readonly IGeoLookupService _geoLookupService;

        public GeoLookupController(
            ILogger<GeoLookupController> logger,
            ITableStorageGeoLocationRepository tableStorage,
            IMaxMindGeoLocationRepository maxMind,
            IHostnameResolver hostnameResolver,
            IGeoLookupService geoLookupService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _maxMind = maxMind ?? throw new ArgumentNullException(nameof(maxMind));
            _hostnameResolver = hostnameResolver ?? throw new ArgumentNullException(nameof(hostnameResolver));
            _geoLookupService = geoLookupService ?? throw new ArgumentNullException(nameof(geoLookupService));
        }

        [HttpGet]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> GetGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return ErrorResult<GeoLocationDto>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME);

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Hostname"] = hostname,
                ["Operation"] = "GetGeoLocation"
            });

            var response = await _geoLookupService.ExecuteLookup<GeoLocationDto>(hostname, cancellationToken, async address =>
            {
                var dto = await _tableStorage.GetGeoLocation(address, cancellationToken);
                if (dto is not null)
                    return new ApiResponse<GeoLocationDto>(dto).ToApiResult();

                dto = await _maxMind.GetGeoLocation(address, cancellationToken);
                dto.Address = hostname;
                await _tableStorage.StoreGeoLocation(dto, cancellationToken);

                return new ApiResponse<GeoLocationDto>(dto).ToApiResult();
            });

            return response.ToHttpResult();
        }

        [HttpPost]
        [Route("lookup")]
        public async Task<IActionResult> GetGeoLocations([FromBody] List<string>? hostnames, CancellationToken cancellationToken)
        {
            if (hostnames is null)
                return ErrorResult<CollectionModel<GeoLocationDto>>(HttpStatusCode.BadRequest, ErrorCodes.NULL_REQUEST, ErrorMessages.NULL_REQUEST);

            if (hostnames.Count == 0)
                return ErrorResult<CollectionModel<GeoLocationDto>>(HttpStatusCode.BadRequest, ErrorCodes.EMPTY_REQUEST_LIST, ErrorMessages.EMPTY_REQUEST_LIST);

            if (hostnames.Count > MaxBatchSize)
                return ErrorResult<CollectionModel<GeoLocationDto>>(HttpStatusCode.BadRequest, ErrorCodes.INVALID_HOSTNAME, $"Batch requests are limited to {MaxBatchSize} hostnames.");

            List<GeoLocationDto> entries = [];
            List<ApiError> errors = [];

            await Parallel.ForEachAsync(
                hostnames,
                new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
                async (hostname, ct) =>
            {
                var lookupResult = await _geoLookupService.ExecuteLookup<GeoLocationDto>(hostname, ct, async address =>
                {
                    var dto = await _tableStorage.GetGeoLocation(address, ct);
                    if (dto is not null)
                        return new ApiResponse<GeoLocationDto>(dto).ToApiResult();

                    dto = await _maxMind.GetGeoLocation(address, ct);
                    dto.Address = hostname;
                    await _tableStorage.StoreGeoLocation(dto, ct);

                    return new ApiResponse<GeoLocationDto>(dto).ToApiResult();
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

            var result = new ApiResponse<CollectionModel<GeoLocationDto>>(
                new CollectionModel<GeoLocationDto> { Items = entries })
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

                var deleted = await _tableStorage.DeleteGeoLocation(address, cancellationToken);

                if (!string.Equals(hostname, address, StringComparison.OrdinalIgnoreCase))
                    deleted |= await _tableStorage.DeleteGeoLocation(hostname, cancellationToken);

                return deleted
                    ? new ApiResponse().ToApiResult().ToHttpResult()
                    : new ApiResponse(new ApiError(ErrorCodes.NOT_FOUND, ErrorMessages.NOT_FOUND)).ToNotFoundResult().ToHttpResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting geolocation data for {Hostname}", hostname);
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
