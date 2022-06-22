using System.Net;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.LookupApi.Abstractions.Interfaces;
using MX.GeoLocation.LookupApi.Abstractions.Models;
using MX.GeoLocation.LookupWebApi.Extensions;

using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "LookupApiUser")]
    public class GeoLookupController : Controller, IGeoLookupApi
    {
        [HttpGet]
        [Route("geolocation/lookup/{hostname}")]
        public async Task<IActionResult> GetGeoLocation(string hostname)
        {
            var response = await ((IGeoLookupApi)this).GetGeoLocation(hostname);

            return response.ToHttpResult();
        }

        Task<ApiResponseDto<GeoLocationDto>> IGeoLookupApi.GetGeoLocation(string hostname)
        {
            throw new NotImplementedException();
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
    }
}
