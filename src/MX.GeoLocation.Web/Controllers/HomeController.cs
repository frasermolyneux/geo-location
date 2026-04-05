using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.Web.Extensions;
using MX.GeoLocation.Web.Models;

namespace MX.GeoLocation.Web.Controllers
{
    public class HomeController : Controller
    {
        private const string UserLocationSessionKey = "UserCityGeoLocationDto";
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

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var sessionDto = httpContextAccessor.HttpContext?.Session.GetObjectFromJson<CityGeoLocationDto>(UserLocationSessionKey);

            if (sessionDto is not null)
                return View(sessionDto);

            var address = GetUsersIpForLookup();

            var lookupResponse = await geoLocationApiClient.GeoLookup.V1_1.GetCityGeoLocation(address.ToString(), cancellationToken);

            if (!lookupResponse.IsSuccess || lookupResponse.IsNotFound || lookupResponse.Result?.Data is null)
            {
                return RedirectToAction("LookupAddress");
            }
            else
            {
                httpContextAccessor.HttpContext?.Session.SetObjectAsJson(UserLocationSessionKey, lookupResponse.Result.Data);
                return View(lookupResponse.Result.Data);
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
                return View("Error", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier) { Message = "An error occurred while processing your request." });

            return View("Error", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier) { Message = message });
        }

        [HttpGet]
        public async Task<IActionResult> LookupAddress(string id, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(new LookupAddressViewModel());

            if (string.IsNullOrWhiteSpace(id))
                return View(new LookupAddressViewModel());

            var model = new LookupAddressViewModel { AddressData = id };
            return await LookupAddress(model, cancellationToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LookupAddress(LookupAddressViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData), "You must provide an address to query against. IP or DNS is acceptable.");
                return View(model);
            }

            if (!await ValidateHostname(model.AddressData, cancellationToken))
            {
                ModelState.AddModelError(nameof(model.AddressData), "The address provided is invalid. IP or DNS is acceptable.");
                return View(model);
            }

            var response = await geoLocationApiClient.GeoLookup.V1_1.GetCityGeoLocation(model.AddressData, cancellationToken);

            if (!response.IsSuccess)
            {
                if (response.Result?.Errors is not null)
                    foreach (var error in response.Result.Errors)
                        ModelState.AddModelError(nameof(model.AddressData), error.Message ?? "An error occurred");
                return View(model);
            }
            else if (response.IsNotFound)
            {
                ModelState.AddModelError(nameof(model.AddressData), "Could not retrieve GeoLocation data for address");
            }
            else
            {
                model.CityGeoLocationDto = response.Result?.Data;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult IntelligenceLookup()
        {
            return View(new IntelligenceLookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IntelligenceLookup(IntelligenceLookupViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData), "You must provide an address to query against. IP or DNS is acceptable.");
                return View(model);
            }

            if (!await ValidateHostname(model.AddressData, cancellationToken))
            {
                ModelState.AddModelError(nameof(model.AddressData), "The address provided is invalid. IP or DNS is acceptable.");
                return View(model);
            }

            var response = await geoLocationApiClient.GeoLookup.V1_1.GetIpIntelligence(model.AddressData, cancellationToken);

            if (!response.IsSuccess)
            {
                if (response.Result?.Errors is not null)
                    foreach (var error in response.Result.Errors)
                        ModelState.AddModelError(nameof(model.AddressData), error.Message ?? "An error occurred");
                return View(model);
            }
            else if (response.IsNotFound)
            {
                ModelState.AddModelError(nameof(model.AddressData), "Could not retrieve intelligence data for address");
            }
            else
            {
                model.Intelligence = response.Result?.Data;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult BatchLookup()
        {
            var addressData = httpContextAccessor.HttpContext?.Session.GetString(BatchLookupSessionKey);

            if (!string.IsNullOrWhiteSpace(addressData))
                return View(new BatchLookupViewModel { AddressData = addressData });

            return View(new BatchLookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchLookup(BatchLookupViewModel model, CancellationToken cancellationToken)
        {
            if (model.AddressData is not null)
                httpContextAccessor.HttpContext?.Session.SetString(BatchLookupSessionKey, model.AddressData);

            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.AddressData))
            {
                ModelState.AddModelError(nameof(model.AddressData), "You must provide a line separated list of addresses. IP or DNS is acceptable.");
                return View(model);
            }

            List<string> addresses;
            try
            {
                addresses = model.AddressData
                    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                    .Where(address => !string.IsNullOrWhiteSpace(address))
                    .Select(address => address.Trim())
                    .ToList();
            }
            catch (FormatException)
            {
                ModelState.AddModelError(nameof(model.AddressData), "Invalid data, you must provide a line separated list of addresses. IP or DNS is acceptable.");
                return View(model);
            }

            if (addresses.Count > 20)
            {
                ModelState.AddModelError(nameof(model.AddressData), "You can only search for a maximum of 20 addresses in one request");
                return View(model);
            }

            var lookupResponse = await geoLocationApiClient.GeoLookup.V1_1.GetIpIntelligences(addresses, cancellationToken);

            if (!lookupResponse.IsSuccess || (lookupResponse.Result?.Errors?.Any() ?? false))
            {
                if (lookupResponse.Result?.Errors is not null)
                    foreach (var error in lookupResponse.Result.Errors)
                        ModelState.AddModelError(nameof(model.AddressData), error.Message ?? "An error occurred");
            }

            if (lookupResponse.IsSuccess)
                model.IntelligenceCollectionDto = lookupResponse.Result?.Data;

            return View(model);
        }

        [HttpGet]
        public IActionResult RemoveData()
        {
            var model = new RemoveMyDataViewModel(GetUsersIpForLookup().ToString());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveData(RemoveMyDataViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (!await ValidateHostname(model.AddressData, cancellationToken))
                {
                    ModelState.AddModelError(nameof(model.AddressData), "The address provided is invalid. IP or DNS is acceptable.");
                    return View(model);
                }

                var deleteResponse = await geoLocationApiClient.GeoLookup.V1_1.DeleteMetadata(model.AddressData, cancellationToken);

                if (!deleteResponse.IsSuccess)
                {
                    if (deleteResponse.Result?.Errors is not null)
                        foreach (var error in deleteResponse.Result.Errors)
                            ModelState.AddModelError(nameof(model.AddressData), error.Message ?? "An error occurred");
                    return View(model);
                }

                model.Removed = true;
                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(nameof(model.AddressData), "An error occurred while trying to remove the data. Please try again.");
                return View(model);
            }
        }

        private IPAddress GetUsersIpForLookup()
        {
            const string cfConnectingIpKey = "CF-Connecting-IP";

            if (webHostEnvironment.IsDevelopment())
                return IPAddress.Parse("8.8.8.8");

            IPAddress? address = null;

            if (httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(cfConnectingIpKey, out var cfConnectingIp) ?? false)
                IPAddress.TryParse(cfConnectingIp, out address);

            if (address is null)
                address = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

            return address ?? IPAddress.Parse("8.8.8.8");
        }

        private async Task<bool> ValidateHostname(string address, CancellationToken cancellationToken)
        {
            if (IPAddress.TryParse(address, out _))
                return true;

            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(address, cancellationToken);
                return hostEntry.AddressList.FirstOrDefault() is not null;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
