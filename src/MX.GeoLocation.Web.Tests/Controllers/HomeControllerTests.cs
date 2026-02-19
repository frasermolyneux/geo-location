using System.Net;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Api.Client.Testing;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Web.Controllers;

using Newtonsoft.Json;

namespace MX.GeoLocation.Web.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly FakeGeoLocationApiClient fakeGeoLocationClient;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<IWebHostEnvironment> mockWebHostEnvironment;

        private readonly HomeController homeController;

        private readonly GeoLocationDto wellFormedGeoLocationDto;

        public HomeControllerTests()
        {
            fakeGeoLocationClient = new FakeGeoLocationApiClient();
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            homeController = new HomeController(fakeGeoLocationClient, mockHttpContextAccessor.Object, mockWebHostEnvironment.Object);

            wellFormedGeoLocationDto = GeoLocationDtoFactory.CreateGeoLocation(
                address: "81.174.169.65",
                cityName: "Chesterfield",
                continentCode: "EU",
                continentName: "Europe",
                countryCode: "GB",
                countryName: "United Kingdom",
                isEuropeanUnion: false,
                latitude: 53.2852,
                longitude: -1.2899,
                postalCode: "S43",
                registeredCountry: "GB",
                accuracyRadius: 200,
                timezone: "Europe/London",
                traits: new()
                {
                    { "AutonomousSystemNumber", "6871" },
                    { "ConnectionType", null },
                    { "Isp", "Plusnet" }
                });
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task IndexShouldRedirectToLookupAddressWhenGetGeoLocationFails(HttpStatusCode httpStatusCode)
        {
            // Arrange - configure the fake to return an error for any address
            fakeGeoLocationClient.V1Lookup.AddErrorResponse("8.8.8.8", httpStatusCode, "ERROR", "Test error");

            var mockConnection = new Mock<ConnectionInfo>();
            mockConnection.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("8.8.8.8"));
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());
            byte[]? nullSessionData = null;
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("UserGeoLocationDto", out nullSessionData)).Returns(false);
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            // Act
            var result = await homeController.Index(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("LookupAddress", redirectToActionResult!.ActionName);
        }

        [Fact]
        public async Task IndexShouldUseGeoLocationDtoFromSessionWhenItIsNotNull()
        {
            // Arrange
            byte[]? sessionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wellFormedGeoLocationDto));
            var mockSession = new Mock<ISession>();
            byte[]? outData = sessionData;
            mockSession.Setup(s => s.TryGetValue("UserGeoLocationDto", out outData)).Returns(true);
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            // Act
            var result = await homeController.Index(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult!.Model);
            Assert.IsType<GeoLocationDto>(viewResult.Model);

            var viewResultGeoLocationDto = viewResult.Model as GeoLocationDto;
            Assert.NotNull(viewResultGeoLocationDto);
            AssertGeoLocationDtoEquals(wellFormedGeoLocationDto, viewResultGeoLocationDto!);
        }

        [Fact]
        public async Task IndexShouldGetGeoLocationAndStoreInSessionIfSessionDataIsNull()
        {
            // Arrange
            fakeGeoLocationClient.V1Lookup.AddResponse("8.8.8.8", wellFormedGeoLocationDto);

            byte[]? nullSessionData = null;
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("UserGeoLocationDto", out nullSessionData)).Returns(false);
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());
            var mockConnection = new Mock<ConnectionInfo>();
            mockConnection.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("8.8.8.8"));
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            // Act
            var result = await homeController.Index(CancellationToken.None);

            // Assert
            mockSession.Verify(s => s.Set("UserGeoLocationDto", It.IsAny<byte[]>()), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult!.Model);
            Assert.IsType<GeoLocationDto>(viewResult.Model);

            var viewResultGeoLocationDto = viewResult.Model as GeoLocationDto;
            Assert.NotNull(viewResultGeoLocationDto);
            AssertGeoLocationDtoEquals(wellFormedGeoLocationDto, viewResultGeoLocationDto!);
        }

        private static void AssertGeoLocationDtoEquals(GeoLocationDto expected, GeoLocationDto actual)
        {
            Assert.Equal(expected.AccuracyRadius, actual.AccuracyRadius);
            Assert.Equal(expected.Address, actual.Address);
            Assert.Equal(expected.CityName, actual.CityName);
            Assert.Equal(expected.ContinentCode, actual.ContinentCode);
            Assert.Equal(expected.ContinentName, actual.ContinentName);
            Assert.Equal(expected.CountryCode, actual.CountryCode);
            Assert.Equal(expected.CountryName, actual.CountryName);
            Assert.Equal(expected.IsEuropeanUnion, actual.IsEuropeanUnion);
            Assert.Equal(expected.Latitude, actual.Latitude);
            Assert.Equal(expected.Longitude, actual.Longitude);
            Assert.Equal(expected.PostalCode, actual.PostalCode);
            Assert.Equal(expected.RegisteredCountry, actual.RegisteredCountry);
            Assert.Equal(expected.RepresentedCountry, actual.RepresentedCountry);
            Assert.Equal(expected.Timezone, actual.Timezone);
            Assert.Equal(expected.Traits, actual.Traits);
        }
    }
}
