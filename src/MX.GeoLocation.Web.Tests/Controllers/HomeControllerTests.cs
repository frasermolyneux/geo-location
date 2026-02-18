using System.Net;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Web.Controllers;

using MX.Api.Abstractions;

using Newtonsoft.Json;

namespace MX.GeoLocation.Web.Tests.Controllers
{
    public class HomeControllerTests : IDisposable
    {
        private readonly Mock<IGeoLocationApiClient> mockGeoLocationClient;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<IWebHostEnvironment> mockWebHostEnvironment;

        private readonly HomeController homeController;

        private readonly GeoLocationDto wellFormedGeoLocationDto;

        public HomeControllerTests()
        {
            mockGeoLocationClient = new Mock<IGeoLocationApiClient>();
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            homeController = new HomeController(mockGeoLocationClient.Object, mockHttpContextAccessor.Object, mockWebHostEnvironment.Object);

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
                Traits = new()
                {
                    { "AutonomousSystemNumber", "6871" },
                    { "ConnectionType", null },
                    { "Isp", "Plusnet" }
                }
            };
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task IndexShouldRedirectToLookupAddressWhenGetGeoLocationFails(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var mockGeoLookup = new Mock<IVersionedGeoLookupApi>();
            var mockV1 = new Mock<IGeoLookupApi>();
            mockV1.Setup(x => x.GetGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResult<GeoLocationDto>(httpStatusCode));
            mockGeoLookup.Setup(x => x.V1).Returns(mockV1.Object);
            mockGeoLocationClient.Setup(x => x.GeoLookup).Returns(mockGeoLookup.Object);

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
            Assert.Equal(wellFormedGeoLocationDto.AccuracyRadius, viewResultGeoLocationDto!.AccuracyRadius);
            Assert.Equal(wellFormedGeoLocationDto.Address, viewResultGeoLocationDto.Address);
            Assert.Equal(wellFormedGeoLocationDto.CityName, viewResultGeoLocationDto.CityName);
            Assert.Equal(wellFormedGeoLocationDto.ContinentCode, viewResultGeoLocationDto.ContinentCode);
            Assert.Equal(wellFormedGeoLocationDto.ContinentName, viewResultGeoLocationDto.ContinentName);
            Assert.Equal(wellFormedGeoLocationDto.CountryCode, viewResultGeoLocationDto.CountryCode);
            Assert.Equal(wellFormedGeoLocationDto.CountryName, viewResultGeoLocationDto.CountryName);
            Assert.Equal(wellFormedGeoLocationDto.IsEuropeanUnion, viewResultGeoLocationDto.IsEuropeanUnion);
            Assert.Equal(wellFormedGeoLocationDto.Latitude, viewResultGeoLocationDto.Latitude);
            Assert.Equal(wellFormedGeoLocationDto.Longitude, viewResultGeoLocationDto.Longitude);
            Assert.Equal(wellFormedGeoLocationDto.PostalCode, viewResultGeoLocationDto.PostalCode);
            Assert.Equal(wellFormedGeoLocationDto.RegisteredCountry, viewResultGeoLocationDto.RegisteredCountry);
            Assert.Equal(wellFormedGeoLocationDto.RepresentedCountry, viewResultGeoLocationDto.RepresentedCountry);
            Assert.Equal(wellFormedGeoLocationDto.Timezone, viewResultGeoLocationDto.Timezone);
            Assert.Equal(wellFormedGeoLocationDto.Traits, viewResultGeoLocationDto.Traits);
        }

        [Fact]
        public async Task IndexShouldGetGeoLocationAndStoreInSessionIfSessionDataIsNull()
        {
            // Arrange
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

            var mockGeoLookup = new Mock<IVersionedGeoLookupApi>();
            var mockV1 = new Mock<IGeoLookupApi>();
            mockV1.Setup(x => x.GetGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(wellFormedGeoLocationDto)));
            mockGeoLookup.Setup(x => x.V1).Returns(mockV1.Object);
            mockGeoLocationClient.Setup(x => x.GeoLookup).Returns(mockGeoLookup.Object);

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
            Assert.Equal(wellFormedGeoLocationDto.AccuracyRadius, viewResultGeoLocationDto!.AccuracyRadius);
            Assert.Equal(wellFormedGeoLocationDto.Address, viewResultGeoLocationDto.Address);
            Assert.Equal(wellFormedGeoLocationDto.CityName, viewResultGeoLocationDto.CityName);
            Assert.Equal(wellFormedGeoLocationDto.ContinentCode, viewResultGeoLocationDto.ContinentCode);
            Assert.Equal(wellFormedGeoLocationDto.ContinentName, viewResultGeoLocationDto.ContinentName);
            Assert.Equal(wellFormedGeoLocationDto.CountryCode, viewResultGeoLocationDto.CountryCode);
            Assert.Equal(wellFormedGeoLocationDto.CountryName, viewResultGeoLocationDto.CountryName);
            Assert.Equal(wellFormedGeoLocationDto.IsEuropeanUnion, viewResultGeoLocationDto.IsEuropeanUnion);
            Assert.Equal(wellFormedGeoLocationDto.Latitude, viewResultGeoLocationDto.Latitude);
            Assert.Equal(wellFormedGeoLocationDto.Longitude, viewResultGeoLocationDto.Longitude);
            Assert.Equal(wellFormedGeoLocationDto.PostalCode, viewResultGeoLocationDto.PostalCode);
            Assert.Equal(wellFormedGeoLocationDto.RegisteredCountry, viewResultGeoLocationDto.RegisteredCountry);
            Assert.Equal(wellFormedGeoLocationDto.RepresentedCountry, viewResultGeoLocationDto.RepresentedCountry);
            Assert.Equal(wellFormedGeoLocationDto.Timezone, viewResultGeoLocationDto.Timezone);
            Assert.Equal(wellFormedGeoLocationDto.Traits, viewResultGeoLocationDto.Traits);
        }

        public void Dispose()
        {
            homeController?.Dispose();
        }
    }
}
