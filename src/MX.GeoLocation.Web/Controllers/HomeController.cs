using System.Diagnostics;
using System.Net;

using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Web.Extensions;
using MX.GeoLocation.Web.Models;

namespace MX.GeoLocation.Web.Controllers
{
    public class HomeController : Controller
    {
        private const string UserLocationSessionKey = "UserGeoLocationDto";
        private const string BatchLookupSessionKey = "BatchLookupAddressData";

        private readonly IGeoLocationApiClient geoLocationApiClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(
            IGeoLocationApiClient geoLocationClient,
            IHttpContextAccessor httpContext,
            IWebHostEnvironment environment)
        {
            geoLocationApiClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            httpContextAccessor = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            webHostEnvironment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<IActionResult> Index()
        {
            var sessionGeoLocationDto = httpContextAccessor.HttpContext?.Session.GetObjectFromJson<GeoLocationDto>(UserLocationSessionKey);

            if (sessionGeoLocationDto != null)
                return View(sessionGeoLocationDto);

            var address = GetUsersIpForLookup();

            var lookupAddressResponse = await geoLocationApiClient.GeoLookup.GetGeoLocation(address.ToString());

            if (!lookupAddressResponse.IsSuccess || lookupAddressResponse.IsNotFound || lookupAddressResponse.Result == null)
            {
                return RedirectToAction("LookupAddress");
            }
            else
            {
                httpContextAccessor.HttpContext?.Session.SetObjectAsJson(UserLocationSessionKey, lookupAddressResponse.Result);

                return View(lookupAddressResponse.Result);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string? message)
        {
            if (!ModelState.IsValid)
            {
                return View("Error", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier) { Message = "An error occurred while processing your request." });
            }

            return View("Error", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier) { Message = message });
        }

        [HttpGet]
        public async Task<IActionResult> LookupAddress(string id)
        {
            if (!ModelState.IsValid)
                return View(new LookupAddressViewModel());

            if (string.IsNullOrWhiteSpace(id))
                return View(new LookupAddressViewModel());

            var model = new LookupAddressViewModel
            {
                AddressData = id
            };

            return await LookupAddress(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LookupAddress(LookupAddressViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData),
                    "You must provide an address to query against. IP or DNS is acceptable.");
                return View(model);
            }

            if (!ValidateHostname(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData), "The address provided is invalid. IP or DNS is acceptable.");
                return View(model);
            }

            var lookupAddressResponse = await geoLocationApiClient.GeoLookup.GetGeoLocation(model.AddressData);

            if (!lookupAddressResponse.IsSuccess)
            {
                lookupAddressResponse.Errors.ForEach(error =>
                {
                    ModelState.AddModelError(nameof(model.AddressData), error);
                });

                return View(model);
            }
            else if (lookupAddressResponse.IsNotFound)
            {
                ModelState.AddModelError(nameof(model.AddressData), "Could not retrieve GeoLocation data for address");
            }
            else
            {
                model.GeoLocationDto = lookupAddressResponse.Result;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult BatchLookup()
        {
            var addressData = httpContextAccessor.HttpContext?.Session.GetString(BatchLookupSessionKey);

            if (!string.IsNullOrWhiteSpace(addressData))
                return View(new BatchLookupViewModel
                {
                    AddressData = addressData
                });

            return View(new BatchLookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchLookup(BatchLookupViewModel model)
        {
            if (model.AddressData != null)
                httpContextAccessor.HttpContext?.Session.SetString(BatchLookupSessionKey, model.AddressData);

            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData),
                    "You must provide a line separated list of addresses. IP or DNS is acceptable.");
                return View(model);
            }

            List<string> addresses;
            try
            {
                addresses = model.AddressData.Split(Environment.NewLine).ToList();
            }
            catch
            {
                ModelState.AddModelError(nameof(model.AddressData),
                    "Invalid data, you must provide a line separated list of addresses. IP or DNS is acceptable.");
                return View(model);
            }

            if (addresses.Count >= 20)
            {
                ModelState.AddModelError(nameof(model.AddressData),
                    "You can only search for a maximum of 20 addresses in one request");
                return View(model);
            }

            var lookupAddressesResponse = await geoLocationApiClient.GeoLookup.GetGeoLocations(addresses);

            if (!lookupAddressesResponse.IsSuccess || lookupAddressesResponse.Errors.Any())
            {
                lookupAddressesResponse.Errors.ForEach(error =>
                {
                    ModelState.AddModelError(nameof(model.AddressData), error);
                });
            }

            if (lookupAddressesResponse.IsSuccess)
            {
                model.GeoLocationCollectionDto = lookupAddressesResponse.Result;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult RemoveData()
        {
            var model = new RemoveMyDataViewModel(GetUsersIpForLookup().ToString());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveData(RemoveMyDataViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData),
                    "You must provide an address to query against. IP or DNS is acceptable.");
                return View(model);
            }

            if (!ValidateHostname(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData), "The address provided is invalid. IP or DNS is acceptable.");
                return View(model);
            }

            var deleteMetaDataResponse = await geoLocationApiClient.GeoLookup.DeleteMetadata(model.AddressData);

            if (!deleteMetaDataResponse.IsSuccess)
            {
                deleteMetaDataResponse.Errors.ForEach(error =>
                {
                    ModelState.AddModelError(nameof(model.AddressData), error);
                });

                return View(model);
            }
            else
            {
                model.Removed = true;
            }


            return View(model);
        }

        private IPAddress GetUsersIpForLookup()
        {
            const string cfConnectingIpKey = "CF-Connecting-IP";
            const string xForwardedForHeaderKey = "X-Forwarded-For";

            if (webHostEnvironment.IsDevelopment())
                return IPAddress.Parse("8.8.8.8");

            IPAddress? address = null;

            if (httpContextAccessor.HttpContext?.Request.Headers.ContainsKey(cfConnectingIpKey) == true)
            {
                var cfConnectingIp = httpContextAccessor.HttpContext.Request.Headers[cfConnectingIpKey];
                IPAddress.TryParse(cfConnectingIp, out address);
            }

            if (address == null && httpContextAccessor.HttpContext?.Request.Headers.ContainsKey(xForwardedForHeaderKey) == true)
            {
                var forwardedAddress = httpContextAccessor.HttpContext.Request.Headers[xForwardedForHeaderKey];
                IPAddress.TryParse(forwardedAddress, out address);
            }

            if (address == null)
                address = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

            return address ?? IPAddress.Parse("8.8.8.8");
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
    }
}