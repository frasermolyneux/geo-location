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
        private readonly ITableStorageGeoLocationRepository tableStorageGeoLocationRepository;
        private readonly IMaxMindGeoLocationRepository maxMindGeoLocationRepository;

        private readonly string[] localOverrides = { "localhost", "127.0.0.1" };

        public GeoLookupController(
            ITableStorageGeoLocationRepository tableStorageGeoLocationRepository,
            IMaxMindGeoLocationRepository maxMindGeoLocationRepository)
        {
            this.tableStorageGeoLocationRepository = tableStorageGeoLocationRepository;
            this.maxMindGeoLocationRepository = maxMindGeoLocationRepository ?? throw new ArgumentNullException(nameof(maxMindGeoLocationRepository));
        }

        [HttpGet]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> GetGeoLocation(string hostname)
        {
            if (!ValidateHostname(hostname))
            {
                return new ApiResponse<GeoLocationDto>(new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME)).ToBadRequestResult().ToHttpResult();
            }

            var response = await ((IGeoLookupApi)this).GetGeoLocation(hostname);

            return response.ToHttpResult();
        }

        async Task<ApiResult<GeoLocationDto>> IGeoLookupApi.GetGeoLocation(string hostname)
        {
            try
            {
                if (ConvertHostname(hostname, out var validatedAddress) && validatedAddress != null)
                {
                    if (localOverrides.Contains(hostname))
                    {
                        return new ApiResponse<GeoLocationDto>(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS)).ToNotFoundResult();
                    }

                    var geoLocationDto = await tableStorageGeoLocationRepository.GetGeoLocation(validatedAddress);

                    if (geoLocationDto != null)
                        return new ApiResponse<GeoLocationDto>(geoLocationDto).ToApiResult();

                    geoLocationDto = await maxMindGeoLocationRepository.GetGeoLocation(validatedAddress);
                    geoLocationDto.Address = hostname; // Set the address to be the original hostname query

                    await tableStorageGeoLocationRepository.StoreGeoLocation(geoLocationDto);

                    return new ApiResponse<GeoLocationDto>(geoLocationDto).ToApiResult();
                }
                else
                {
                    return new ApiResponse<GeoLocationDto>(new ApiError(ErrorCodes.HOSTNAME_RESOLUTION_FAILED, ErrorMessages.HOSTNAME_RESOLUTION_FAILED)).ToApiResult(HttpStatusCode.InternalServerError);
                }
            }
            catch (AddressNotFoundException)
            {
                return new ApiResponse<GeoLocationDto>(new ApiError(ErrorCodes.ADDRESS_NOT_FOUND, ErrorMessages.ADDRESS_NOT_FOUND)).ToNotFoundResult();
            }
            catch (GeoIP2Exception ex)
            {
                return new ApiResponse<GeoLocationDto>(new ApiError(ErrorCodes.GEOIP_ERROR, ex.Message)).ToBadRequestResult();
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

            var response = await ((IGeoLookupApi)this).GetGeoLocations(hostnames);

            return response.ToHttpResult();
        }

        async Task<ApiResult<CollectionModel<GeoLocationDto>>> IGeoLookupApi.GetGeoLocations(List<string> hostnames)
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

            var result = new CollectionModel<GeoLocationDto>
            {
                Items = entries,
                TotalCount = entries.Count,
                FilteredCount = entries.Count
            };

            var response = new ApiResponse<CollectionModel<GeoLocationDto>>(result) { Errors = errors.ToArray() };
            return response.ToApiResult();
        }

        [HttpDelete]
        [Route("lookup/{hostname}")]
        public async Task<IActionResult> DeleteMetadata(string hostname)
        {
            var response = await ((IGeoLookupApi)this).DeleteMetadata(hostname);

            return response.ToHttpResult();
        }

        Task<ApiResult> IGeoLookupApi.DeleteMetadata(string hostname)
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
