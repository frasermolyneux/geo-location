using System.Net;
using System.Net.Sockets;

using MaxMind.GeoIP2.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

using MX.Api.Abstractions;
using MX.Api.Web.Extensions;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Constants;
using MX.GeoLocation.LookupWebApi.Repositories;

using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}")]
    [Authorize(Roles = "LookupApiUser")]
    public class GeoLookupController : Controller, IGeoLookupApi
    {
        private readonly ILogger<GeoLookupController> _logger;
        private readonly ITableStorageGeoLocationRepository tableStorageGeoLocationRepository;
        private readonly IMaxMindGeoLocationRepository maxMindGeoLocationRepository;

        private readonly string[] localOverrides = { "localhost", "127.0.0.1" };

        public GeoLookupController(
            ILogger<GeoLookupController> logger,
            ITableStorageGeoLocationRepository tableStorageGeoLocationRepository,
            IMaxMindGeoLocationRepository maxMindGeoLocationRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tableStorageGeoLocationRepository = tableStorageGeoLocationRepository ?? throw new ArgumentNullException(nameof(tableStorageGeoLocationRepository));
            this.maxMindGeoLocationRepository = maxMindGeoLocationRepository ?? throw new ArgumentNullException(nameof(maxMindGeoLocationRepository));
        }

        [HttpGet]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> GetGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                var badRequestResponse = new ApiResponse<GeoLocationDto>(
                    new ApiError(ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME));
                return badRequestResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
            }

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Hostname"] = hostname,
                ["Operation"] = "GetGeoLocation"
            });

            _logger.LogInformation("Getting geolocation for hostname {Hostname}", hostname);

            if (!await ValidateHostname(hostname, cancellationToken))
            {
                _logger.LogWarning("Invalid hostname provided: {Hostname}", hostname);
                var errorResponse = new ApiResponse<GeoLocationDto>(
                    new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME));
                return errorResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
            }

            var response = await ((IGeoLookupApi)this).GetGeoLocation(hostname, cancellationToken);
            return response.ToHttpResult();
        }

        async Task<ApiResult<GeoLocationDto>> IGeoLookupApi.GetGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing geolocation lookup for {Hostname}", hostname);

                var (convertSuccess, validatedAddress) = await ConvertHostname(hostname, cancellationToken);
                if (convertSuccess && validatedAddress is not null)
                {
                    if (localOverrides.Contains(hostname))
                    {
                        _logger.LogWarning("Local address lookup attempted: {Hostname}", hostname);
                        return new ApiResponse<GeoLocationDto>(
                            new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS))
                            .ToApiResult(HttpStatusCode.BadRequest);
                    }

                    // Try cache first
                    var geoLocationDto = await tableStorageGeoLocationRepository.GetGeoLocation(validatedAddress, cancellationToken);

                    if (geoLocationDto is not null)
                    {
                        _logger.LogInformation("Found cached geolocation data for {ValidatedAddress}", validatedAddress);
                        return new ApiResponse<GeoLocationDto>(geoLocationDto).ToApiResult();
                    }

                    // Fallback to MaxMind
                    _logger.LogInformation("No cached data found, querying MaxMind for {ValidatedAddress}", validatedAddress);
                    geoLocationDto = await maxMindGeoLocationRepository.GetGeoLocation(validatedAddress, cancellationToken);
                    geoLocationDto.Address = hostname; // Set the address to be the original hostname query

                    await tableStorageGeoLocationRepository.StoreGeoLocation(geoLocationDto, cancellationToken);
                    _logger.LogInformation("Successfully stored geolocation data for {ValidatedAddress}", validatedAddress);

                    return new ApiResponse<GeoLocationDto>(geoLocationDto).ToApiResult();
                }
                else
                {
                    _logger.LogError("Hostname resolution failed for {Hostname}", hostname);
                    return new ApiResponse<GeoLocationDto>(
                        new ApiError(ErrorCodes.HOSTNAME_RESOLUTION_FAILED, ErrorMessages.HOSTNAME_RESOLUTION_FAILED))
                        .ToApiResult(HttpStatusCode.BadRequest);
                }
            }
            catch (AddressNotFoundException ex)
            {
                _logger.LogWarning(ex, "Address not found for {Hostname}", hostname);
                return new ApiResponse<GeoLocationDto>(
                    new ApiError(ErrorCodes.ADDRESS_NOT_FOUND, ErrorMessages.ADDRESS_NOT_FOUND))
                    .ToApiResult(HttpStatusCode.NotFound);
            }
            catch (GeoIP2Exception ex)
            {
                _logger.LogError(ex, "GeoIP2 error for {Hostname}", hostname);
                return new ApiResponse<GeoLocationDto>(
                    new ApiError(ErrorCodes.GEOIP_ERROR, ex.Message))
                    .ToApiResult(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during geolocation lookup for {Hostname}", hostname);
                return new ApiResponse<GeoLocationDto>(
                    new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred"))
                    .ToApiResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Route("lookup")]
        public async Task<IActionResult> GetGeoLocations(CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            List<string>? hostnames;
            try
            {
                hostnames = JsonConvert.DeserializeObject<List<string>>(requestBody);
            }
            catch (JsonException)
            {
                return new ApiResponse<CollectionModel<GeoLocationDto>>(new ApiError(ErrorCodes.INVALID_JSON, ErrorMessages.INVALID_JSON)).ToBadRequestResult().ToHttpResult();
            }

            if (hostnames is null)
                return new ApiResponse<CollectionModel<GeoLocationDto>>(new ApiError(ErrorCodes.NULL_REQUEST, ErrorMessages.NULL_REQUEST)).ToBadRequestResult().ToHttpResult();

            if (hostnames.Count == 0)
                return new ApiResponse<CollectionModel<GeoLocationDto>>(new ApiError(ErrorCodes.EMPTY_REQUEST_LIST, ErrorMessages.EMPTY_REQUEST_LIST)).ToBadRequestResult().ToHttpResult();

            var response = await ((IGeoLookupApi)this).GetGeoLocations(hostnames, cancellationToken);

            return response.ToHttpResult();
        }

        async Task<ApiResult<CollectionModel<GeoLocationDto>>> IGeoLookupApi.GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken)
        {
            List<GeoLocationDto> entries = [];
            List<ApiError> errors = [];

            foreach (var hostname in hostnames)
            {
                try
                {
                    var (convertSuccess, validatedAddress) = await ConvertHostname(hostname, cancellationToken);
                    if (convertSuccess && validatedAddress is not null)
                    {
                        if (localOverrides.Contains(hostname))
                        {
                            errors.Add(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS_BATCH));
                            continue;
                        }

                        var geoLocationDto = await tableStorageGeoLocationRepository.GetGeoLocation(validatedAddress, cancellationToken);

                        if (geoLocationDto is not null)
                            entries.Add(geoLocationDto);
                        else
                        {
                            geoLocationDto = await maxMindGeoLocationRepository.GetGeoLocation(validatedAddress, cancellationToken);
                            geoLocationDto.Address = hostname; // Set the address to be the original hostname query

                            entries.Add(geoLocationDto);

                            await tableStorageGeoLocationRepository.StoreGeoLocation(geoLocationDto, cancellationToken);
                        }
                    }
                    else
                    {
                        errors.Add(new ApiError(ErrorCodes.INVALID_HOSTNAME, $"The hostname provided '{hostname}' is invalid"));
                    }
                }
                catch (AddressNotFoundException ex)
                {
                    errors.Add(new ApiError(ErrorCodes.ADDRESS_NOT_FOUND, ex.Message));
                }
                catch (GeoIP2Exception ex)
                {
                    errors.Add(new ApiError(ErrorCodes.GEOIP_ERROR, ex.Message));
                }
            }

            var data = new CollectionModel<GeoLocationDto>
            {
                Items = entries
            };

            return new ApiResponse<CollectionModel<GeoLocationDto>>(data)
            {
                Errors = [..errors],
                Pagination = new ApiPagination(entries.Count, entries.Count, 0, 0)
            }.ToApiResult();
        }

        [HttpDelete]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> DeleteMetadata(string hostname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                var badRequestResponse = new ApiResponse(new ApiError(ErrorCodes.EMPTY_HOSTNAME, ErrorMessages.EMPTY_HOSTNAME));
                return badRequestResponse.ToBadRequestResult().ToHttpResult();
            }

            var response = await ((IGeoLookupApi)this).DeleteMetadata(hostname, cancellationToken);

            return response.ToHttpResult();
        }

        Task<ApiResult> IGeoLookupApi.DeleteMetadata(string hostname, CancellationToken cancellationToken)
        {
            return DeleteMetadataInternal(hostname, cancellationToken);
        }

        private async Task<ApiResult> DeleteMetadataInternal(string hostname, CancellationToken cancellationToken)
        {
            try
            {
                if (!await ValidateHostname(hostname, cancellationToken))
                {
                    return new ApiResponse(new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME)).ToBadRequestResult();
                }

                var (convertSuccess, validatedAddress) = await ConvertHostname(hostname, cancellationToken);
                if (convertSuccess && validatedAddress is not null)
                {
                    if (localOverrides.Contains(hostname))
                    {
                        return new ApiResponse(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS_DELETE)).ToBadRequestResult();
                    }

                    var deletedCount = 0;

                    // Delete by resolved IP address (primary method since RowKey is TranslatedAddress)
                    var deleted = await tableStorageGeoLocationRepository.DeleteGeoLocation(validatedAddress, cancellationToken);
                    if (deleted)
                    {
                        deletedCount++;
                    }

                    // If hostname is different from resolved IP, also try to delete by hostname
                    // (in case there's legacy data stored with hostname as key)
                    if (!string.Equals(hostname, validatedAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        var deletedByHostname = await tableStorageGeoLocationRepository.DeleteGeoLocation(hostname, cancellationToken);
                        if (deletedByHostname)
                        {
                            deletedCount++;
                        }
                    }

                    if (deletedCount > 0)
                    {
                        return new ApiResponse().ToApiResult();
                    }
                    else
                    {
                        return new ApiResponse(new ApiError(ErrorCodes.NOT_FOUND, ErrorMessages.NOT_FOUND)).ToNotFoundResult();
                    }
                }
                else
                {
                    return new ApiResponse(new ApiError(ErrorCodes.HOSTNAME_RESOLUTION_FAILED, ErrorMessages.HOSTNAME_RESOLUTION_FAILED_DELETE)).ToBadRequestResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting geolocation data for {Hostname}", hostname);
                return new ApiResponse(new ApiError(ErrorCodes.INTERNAL_ERROR, "An error occurred while deleting data")).ToApiResult(HttpStatusCode.InternalServerError);
            }
        }

        private async Task<bool> ValidateHostname(string address, CancellationToken cancellationToken)
        {
            if (IPAddress.TryParse(address, out _))
            {
                return true;
            }

            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(address, cancellationToken);

                if (hostEntry.AddressList.FirstOrDefault() is not null)
                {
                    return true;
                }
            }
            catch (SocketException)
            {
                return false;
            }

            return false;
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
