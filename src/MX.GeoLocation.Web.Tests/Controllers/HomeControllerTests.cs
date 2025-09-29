using System.Net;
using System.Text;

using FakeItEasy;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Web.Controllers;

using MX.Api.Abstractions;

using Newtonsoft.Json;

namespace MX.GeoLocation.Web.Tests.Controllers
{
    internal class HomeControllerTests
    {
        private IGeoLocationApiClient fakeGeoLocationClient;
        private IHttpContextAccessor fakeHttpContextAccessor;
        private IWebHostEnvironment fakeWebHostEnvironment;

        private HomeController homeController;

        GeoLocationDto wellFormedGeoLocationDto;

        [SetUp]
        public void Setup()
        {
            fakeGeoLocationClient = A.Fake<IGeoLocationApiClient>();
            fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            fakeWebHostEnvironment = A.Fake<IWebHostEnvironment>();

            homeController = new HomeController(fakeGeoLocationClient, fakeHttpContextAccessor, fakeWebHostEnvironment);

            wellFormedGeoLocationDto = new GeoLocationDto()
            {
                AccuracyRadius = 200,
                Address = "81.174.169.65",
                CityName = "Chesterfield",
                ContinentCode = "EU",
                ContinentName = "Europe",
                CountryCode = "GB",
                CountryName = "United Kingdom",
                IsEuropeanUnion = false,
                Latitude = 53.2852,
                Longitude = -1.2899,
                PostalCode = "S43",
                RegisteredCountry = "GB",
                RepresentedCountry = null,
                Timezone = "Europe/London",
                Traits = new Dictionary<string, string?>()
                {
                    { "AutonomousSystemNumber", "6871" },
                    { "ConnectionType", null },
                    { "Isp", "Plusnet" }
                }
            };
        }

        [TestCase(404)]
        [TestCase(500)]
        public async Task IndexShouldRedirectToLookupAddressWhenGetGeoLocationFails(HttpStatusCode httpStatusCode)
        {
            // Arrange
            A.CallTo(() => fakeGeoLocationClient.GeoLookup.V1.GetGeoLocation(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(new ApiResult<GeoLocationDto>(httpStatusCode)));

            // Act
            var result = await homeController.Index();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.That(redirectToActionResult, Is.Not.Null);
            Assert.That(redirectToActionResult!.ActionName, Is.EqualTo("LookupAddress"));
        }

        [Test]
        public async Task IndexShouldUseGeoLocationDtoFromSessionWhenItIsNotNull()
        {
            // Arrange
            byte[]? sessionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wellFormedGeoLocationDto));
            A.CallTo(() => fakeHttpContextAccessor.HttpContext!.Session.TryGetValue("UserGeoLocationDto", out sessionData)).Returns(true);

            // Act
            var result = await homeController.Index();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ViewResult>());

            var viewResult = result as ViewResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.Not.Null);
            Assert.That(viewResult.Model, Is.InstanceOf<GeoLocationDto>());

            var viewResultGeoLocationDto = viewResult.Model as GeoLocationDto;

            Assert.That(viewResultGeoLocationDto, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewResultGeoLocationDto!.AccuracyRadius, Is.EqualTo(wellFormedGeoLocationDto.AccuracyRadius));
                Assert.That(viewResultGeoLocationDto.Address, Is.EqualTo(wellFormedGeoLocationDto.Address));
                Assert.That(viewResultGeoLocationDto.CityName, Is.EqualTo(wellFormedGeoLocationDto.CityName));
                Assert.That(viewResultGeoLocationDto.ContinentCode, Is.EqualTo(wellFormedGeoLocationDto.ContinentCode));
                Assert.That(viewResultGeoLocationDto.ContinentName, Is.EqualTo(wellFormedGeoLocationDto.ContinentName));
                Assert.That(viewResultGeoLocationDto.CountryCode, Is.EqualTo(wellFormedGeoLocationDto.CountryCode));
                Assert.That(viewResultGeoLocationDto.CountryName, Is.EqualTo(wellFormedGeoLocationDto.CountryName));
                Assert.That(viewResultGeoLocationDto.IsEuropeanUnion, Is.EqualTo(wellFormedGeoLocationDto.IsEuropeanUnion));
                Assert.That(viewResultGeoLocationDto.Latitude, Is.EqualTo(wellFormedGeoLocationDto.Latitude));
                Assert.That(viewResultGeoLocationDto.Longitude, Is.EqualTo(wellFormedGeoLocationDto.Longitude));
                Assert.That(viewResultGeoLocationDto.PostalCode, Is.EqualTo(wellFormedGeoLocationDto.PostalCode));
                Assert.That(viewResultGeoLocationDto.RegisteredCountry, Is.EqualTo(wellFormedGeoLocationDto.RegisteredCountry));
                Assert.That(viewResultGeoLocationDto.RepresentedCountry, Is.EqualTo(wellFormedGeoLocationDto.RepresentedCountry));
                Assert.That(viewResultGeoLocationDto.Timezone, Is.EqualTo(wellFormedGeoLocationDto.Timezone));
                Assert.That(viewResultGeoLocationDto.Traits, Is.EquivalentTo(wellFormedGeoLocationDto.Traits));
            });
        }

        [Test]
        public async Task IndexShouldGetGeoLocationAndStoreInSessionIfSessionDataIsNull()
        {
            // Arrange
            byte[]? nullSessionData = null;
            byte[]? wellFormedSessionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wellFormedGeoLocationDto));
            A.CallTo(() => fakeHttpContextAccessor.HttpContext!.Session.TryGetValue("UserGeoLocationDto", out nullSessionData)).Returns(false);
            A.CallTo(() => fakeGeoLocationClient.GeoLookup.V1.GetGeoLocation(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(wellFormedGeoLocationDto))));

            // Act
            var result = await homeController.Index();

            // Assert
            A.CallTo(() => fakeHttpContextAccessor.HttpContext!.Session.Set("UserGeoLocationDto", A<byte[]>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ViewResult>());

            var viewResult = result as ViewResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.Not.Null);
            Assert.That(viewResult.Model, Is.InstanceOf<GeoLocationDto>());

            var viewResultGeoLocationDto = viewResult.Model as GeoLocationDto;

            Assert.That(viewResultGeoLocationDto, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewResultGeoLocationDto!.AccuracyRadius, Is.EqualTo(wellFormedGeoLocationDto.AccuracyRadius));
                Assert.That(viewResultGeoLocationDto.Address, Is.EqualTo(wellFormedGeoLocationDto.Address));
                Assert.That(viewResultGeoLocationDto.CityName, Is.EqualTo(wellFormedGeoLocationDto.CityName));
                Assert.That(viewResultGeoLocationDto.ContinentCode, Is.EqualTo(wellFormedGeoLocationDto.ContinentCode));
                Assert.That(viewResultGeoLocationDto.ContinentName, Is.EqualTo(wellFormedGeoLocationDto.ContinentName));
                Assert.That(viewResultGeoLocationDto.CountryCode, Is.EqualTo(wellFormedGeoLocationDto.CountryCode));
                Assert.That(viewResultGeoLocationDto.CountryName, Is.EqualTo(wellFormedGeoLocationDto.CountryName));
                Assert.That(viewResultGeoLocationDto.IsEuropeanUnion, Is.EqualTo(wellFormedGeoLocationDto.IsEuropeanUnion));
                Assert.That(viewResultGeoLocationDto.Latitude, Is.EqualTo(wellFormedGeoLocationDto.Latitude));
                Assert.That(viewResultGeoLocationDto.Longitude, Is.EqualTo(wellFormedGeoLocationDto.Longitude));
                Assert.That(viewResultGeoLocationDto.PostalCode, Is.EqualTo(wellFormedGeoLocationDto.PostalCode));
                Assert.That(viewResultGeoLocationDto.RegisteredCountry, Is.EqualTo(wellFormedGeoLocationDto.RegisteredCountry));
                Assert.That(viewResultGeoLocationDto.RepresentedCountry, Is.EqualTo(wellFormedGeoLocationDto.RepresentedCountry));
                Assert.That(viewResultGeoLocationDto.Timezone, Is.EqualTo(wellFormedGeoLocationDto.Timezone));
                Assert.That(viewResultGeoLocationDto.Traits, Is.EquivalentTo(wellFormedGeoLocationDto.Traits));
            });
        }

        [TearDown]
        public void Cleanup()
        {
            homeController?.Dispose();
        }
    }
}
