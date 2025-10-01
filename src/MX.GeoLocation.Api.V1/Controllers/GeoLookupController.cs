using System.Net;

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

namespace MX.GeoLocation.LookupWebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
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
            this.tableStorageGeoLocationRepository = tableStorageGeoLocationRepository;
            this.maxMindGeoLocationRepository = maxMindGeoLocationRepository ?? throw new ArgumentNullException(nameof(maxMindGeoLocationRepository));
        }

        [HttpGet]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> GetGeoLocation(string hostname)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Hostname"] = hostname,
                ["Operation"] = "GetGeoLocation"
            });

            _logger.LogInformation("Getting geolocation for hostname {Hostname}", hostname);

            if (!ValidateHostname(hostname))
            {
                _logger.LogWarning("Invalid hostname provided: {Hostname}", hostname);
                var errorResponse = new ApiResponse<GeoLocationDto>(
                    new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME));
                return errorResponse.ToApiResult(HttpStatusCode.BadRequest).ToHttpResult();
            }

            var response = await ((IGeoLookupApi)this).GetGeoLocation(hostname, default);
            return response.ToHttpResult();
        }

        async Task<ApiResult<GeoLocationDto>> IGeoLookupApi.GetGeoLocation(string hostname, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing geolocation lookup for {Hostname}", hostname);

                if (ConvertHostname(hostname, out var validatedAddress) && validatedAddress != null)
                {
                    if (localOverrides.Contains(hostname))
                    {
                        _logger.LogWarning("Local address lookup attempted: {Hostname}", hostname);
                        return new ApiResponse<GeoLocationDto>(
                            new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS))
                            .ToApiResult(HttpStatusCode.BadRequest);
                    }

                    // Try cache first
                    var geoLocationDto = await tableStorageGeoLocationRepository.GetGeoLocation(validatedAddress);

                    if (geoLocationDto != null)
                    {
                        _logger.LogInformation("Found cached geolocation data for {ValidatedAddress}", validatedAddress);
                        return new ApiResponse<GeoLocationDto>(geoLocationDto).ToApiResult();
                    }

                    // Fallback to MaxMind
                    _logger.LogInformation("No cached data found, querying MaxMind for {ValidatedAddress}", validatedAddress);
                    geoLocationDto = await maxMindGeoLocationRepository.GetGeoLocation(validatedAddress);
                    geoLocationDto.Address = hostname; // Set the address to be the original hostname query

                    await tableStorageGeoLocationRepository.StoreGeoLocation(geoLocationDto);
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
        public async Task<IActionResult> GetGeoLocations()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<string>? hostnames;
            try
            {
                hostnames = JsonConvert.DeserializeObject<List<string>>(requestBody);
            }
            catch
            {
                return new ApiResponse<CollectionModel<GeoLocationDto>>(new ApiError(ErrorCodes.INVALID_JSON, ErrorMessages.INVALID_JSON)).ToBadRequestResult().ToHttpResult();
            }

            if (hostnames == null)
                return new ApiResponse<CollectionModel<GeoLocationDto>>(new ApiError(ErrorCodes.NULL_REQUEST, ErrorMessages.NULL_REQUEST)).ToBadRequestResult().ToHttpResult();

            var response = await ((IGeoLookupApi)this).GetGeoLocations(hostnames, default);

            return response.ToHttpResult();
        }

        async Task<ApiResult<CollectionModel<GeoLocationDto>>> IGeoLookupApi.GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken)
        {
            var entries = new List<GeoLocationDto>();
            var errors = new List<ApiError>();

            foreach (var hostname in hostnames)
            {
                try
                {
                    if (ConvertHostname(hostname, out var validatedAddress) && validatedAddress != null)
                    {
                        if (localOverrides.Contains(hostname))
                        {
                            errors.Add(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS_BATCH));
                            continue;
                        }

                        var geoLocationDto = await tableStorageGeoLocationRepository.GetGeoLocation(validatedAddress);

                        if (geoLocationDto != null)
                            entries.Add(geoLocationDto);
                        else
                        {
                            geoLocationDto = await maxMindGeoLocationRepository.GetGeoLocation(validatedAddress);
                            geoLocationDto.Address = hostname; // Set the address to be the original hostname query

                            entries.Add(geoLocationDto);

                            await tableStorageGeoLocationRepository.StoreGeoLocation(geoLocationDto);
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
                Errors = errors.ToArray(),
                Pagination = new ApiPagination(entries.Count, entries.Count, 0, 0)
            }.ToApiResult();
        }

        [HttpDelete]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> DeleteMetadata(string hostname)
        {
            var response = await ((IGeoLookupApi)this).DeleteMetadata(hostname, default);

            return response.ToHttpResult();
        }

        Task<ApiResult> IGeoLookupApi.DeleteMetadata(string hostname, CancellationToken cancellationToken)
        {
            return DeleteMetadataInternal(hostname);
        }

        private async Task<ApiResult> DeleteMetadataInternal(string hostname)
        {
            try
            {
                if (!ValidateHostname(hostname))
                {
                    return new ApiResponse(new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME)).ToBadRequestResult();
                }

                if (ConvertHostname(hostname, out var validatedAddress) && validatedAddress != null)
                {
                    if (localOverrides.Contains(hostname))
                    {
                        return new ApiResponse(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS_DELETE)).ToBadRequestResult();
                    }

                    var deletedCount = 0;
                    var messages = new List<string>();

                    // Delete by resolved IP address (primary method since RowKey is TranslatedAddress)
                    var deleted = await tableStorageGeoLocationRepository.DeleteGeoLocation(validatedAddress);
                    if (deleted)
                    {
                        deletedCount++;
                        messages.Add($"Deleted data for IP address: {validatedAddress}");
                    }

                    // If hostname is different from resolved IP, also try to delete by hostname
                    // (in case there's legacy data stored with hostname as key)
                    if (!string.Equals(hostname, validatedAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        var deletedByHostname = await tableStorageGeoLocationRepository.DeleteGeoLocation(hostname);
                        if (deletedByHostname)
                        {
                            deletedCount++;
                            messages.Add($"Deleted data for hostname: {hostname}");
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
                return new ApiResponse(new ApiError(ErrorCodes.INTERNAL_ERROR, $"An error occurred while deleting data: {ex.Message}")).ToApiResult(HttpStatusCode.InternalServerError);
            }
        }

        private bool ValidateHostname(string address)
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                return true;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(address);

                if (hostEntry.AddressList.FirstOrDefault() != null)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
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

                if (hostEntry.AddressList.FirstOrDefault() != null)
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
