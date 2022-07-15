using System.Net;

using MaxMind.GeoIP2.Exceptions;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.LookupApi.Abstractions.Interfaces;
using MX.GeoLocation.LookupApi.Abstractions.Models;
using MX.GeoLocation.LookupWebApi.Extensions;
using MX.GeoLocation.LookupWebApi.Repositories;

using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "LookupApiUser")]
    public class GeoLookupController : Controller, IGeoLookupApi
    {
        private readonly ITableStorageGeoLocationRepository tableStorageGeoLocationRepository;
        private readonly IMaxMindGeoLocationRepository maxMindGeoLocationRepository;
        private readonly TelemetryClient telemetryClient;

        public GeoLookupController(
            ITableStorageGeoLocationRepository tableStorageGeoLocationRepository,
            IMaxMindGeoLocationRepository maxMindGeoLocationRepository,
            TelemetryClient telemetryClient)
        {
            this.tableStorageGeoLocationRepository = tableStorageGeoLocationRepository;
            this.maxMindGeoLocationRepository = maxMindGeoLocationRepository ?? throw new ArgumentNullException(nameof(maxMindGeoLocationRepository));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [HttpGet]
        [Route("geolocation/lookup/{hostname}")]
        public async Task<IActionResult> GetGeoLocation(string hostname)
        {
            if (!ValidateHostname(hostname))
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "The address provided is invalid. IP or DNS is acceptable.").ToHttpResult();
            }

            var response = await ((IGeoLookupApi)this).GetGeoLocation(hostname);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<GeoLocationDto>> IGeoLookupApi.GetGeoLocation(string hostname)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("MaxMindQuery");
            operation.Telemetry.Type = $"HTTP";
            operation.Telemetry.Target = $"geoip.maxmind.com";

            try
            {
                if (ConvertHostname(hostname, out var validatedAddress) && validatedAddress != null)
                {
                    var geoLocationDto = await tableStorageGeoLocationRepository.GetGeoLocation(validatedAddress);
                    if (geoLocationDto != null)
                        return new ApiResponseDto<GeoLocationDto>(HttpStatusCode.OK, geoLocationDto);

                    geoLocationDto = await maxMindGeoLocationRepository.GetGeoLocation(validatedAddress);
                    geoLocationDto.Address = hostname;

                    await tableStorageGeoLocationRepository.StoreGeoLocation(geoLocationDto);

                    return new ApiResponseDto<GeoLocationDto>(HttpStatusCode.OK, geoLocationDto);
                }
                else
                {
                    return new ApiResponseDto<GeoLocationDto>(HttpStatusCode.InternalServerError);
                }
            }
            catch (AddressNotFoundException ex)
            {
                return new ApiResponseDto<GeoLocationDto>(HttpStatusCode.NotFound, ex.Message);
            }
            catch (GeoIP2Exception ex)
            {
                return new ApiResponseDto<GeoLocationDto>(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = ex.Message;
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        [HttpPost]
        [Route("geolocation/lookup")]
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
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (hostnames == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

            var response = await ((IGeoLookupApi)this).GetGeoLocations(hostnames);

            return response.ToHttpResult();
        }

        Task<ApiResponseDto<GeoLocationCollectionDto>> IGeoLookupApi.GetGeoLocations(List<string> hostnames)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [Route("geolocation/lookup/{hostname}")]
        public async Task<IActionResult> DeleteMetadata(string hostname)
        {
            var response = await ((IGeoLookupApi)this).DeleteMetadata(hostname);

            return response.ToHttpResult();
        }

        Task<ApiResponseDto> IGeoLookupApi.DeleteMetadata(string hostname)
        {
            throw new NotImplementedException();
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
