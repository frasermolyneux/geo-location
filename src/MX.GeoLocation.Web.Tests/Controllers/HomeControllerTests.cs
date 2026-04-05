using System.Net;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Api.Client.Testing;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.Web.Controllers;

using Newtonsoft.Json;

namespace MX.GeoLocation.Web.Tests.Controllers
{
    [Trait("Category", "Unit")]
    public class HomeControllerTests
    {
        private readonly FakeGeoLocationApiClient fakeGeoLocationClient;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<IWebHostEnvironment> mockWebHostEnvironment;

        private readonly HomeController homeController;

        private readonly CityGeoLocationDto wellFormedCityGeoLocationDto;

        public HomeControllerTests()
        {
            fakeGeoLocationClient = new FakeGeoLocationApiClient();
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            homeController = new HomeController(fakeGeoLocationClient, mockHttpContextAccessor.Object, mockWebHostEnvironment.Object);

            wellFormedCityGeoLocationDto = GeoLocationDtoFactory.CreateCityGeoLocation(
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
                timezone: "Europe/London");
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task IndexShouldRedirectToLookupAddressWhenGetCityGeoLocationFails(HttpStatusCode httpStatusCode)
        {
            // Arrange
            fakeGeoLocationClient.V1_1Lookup.AddCityErrorResponse("8.8.8.8", httpStatusCode, "ERROR", "Test error");

            var mockConnection = new Mock<ConnectionInfo>();
            mockConnection.Setup(c => c.RemoteIpAddress).Returns(IPAddress.Parse("8.8.8.8"));
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());
            byte[]? nullSessionData = null;
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("UserCityGeoLocationDto", out nullSessionData)).Returns(false);
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
        public async Task IndexShouldUseCityGeoLocationDtoFromSessionWhenItIsNotNull()
        {
            // Arrange
            byte[]? sessionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wellFormedCityGeoLocationDto));
            var mockSession = new Mock<ISession>();
            byte[]? outData = sessionData;
            mockSession.Setup(s => s.TryGetValue("UserCityGeoLocationDto", out outData)).Returns(true);
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
            Assert.IsType<CityGeoLocationDto>(viewResult.Model);

            var viewResultDto = viewResult.Model as CityGeoLocationDto;
            Assert.NotNull(viewResultDto);
            AssertCityGeoLocationDtoEquals(wellFormedCityGeoLocationDto, viewResultDto!);
        }

        [Fact]
        public async Task IndexShouldGetCityGeoLocationAndStoreInSessionIfSessionDataIsNull()
        {
            // Arrange
            fakeGeoLocationClient.V1_1Lookup.AddCityResponse("8.8.8.8", wellFormedCityGeoLocationDto);

            byte[]? nullSessionData = null;
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.TryGetValue("UserCityGeoLocationDto", out nullSessionData)).Returns(false);
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());
            var mockConnection = new Mock<ConnectionInfo>();
            mockConnection.Setup(c => c.RemoteIpAddress).Returns(IPAddress.Parse("8.8.8.8"));
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            // Act
            var result = await homeController.Index(CancellationToken.None);

            // Assert
            mockSession.Verify(s => s.Set("UserCityGeoLocationDto", It.IsAny<byte[]>()), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult!.Model);
            Assert.IsType<CityGeoLocationDto>(viewResult.Model);

            var viewResultDto = viewResult.Model as CityGeoLocationDto;
            Assert.NotNull(viewResultDto);
            AssertCityGeoLocationDtoEquals(wellFormedCityGeoLocationDto, viewResultDto!);
        }

        [Fact]
        public async Task IntelligenceLookupGetShouldReturnView()
        {
            // Act
            var result = homeController.IntelligenceLookup();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task IntelligenceLookupPostShouldReturnIntelligenceData()
        {
            // Arrange
            var intelligenceDto = GeoLocationDtoFactory.CreateIpIntelligence(address: "8.8.8.8");
            fakeGeoLocationClient.V1_1Lookup.AddIntelligenceResponse("8.8.8.8", intelligenceDto);

            var model = new MX.GeoLocation.Web.Models.IntelligenceLookupViewModel { AddressData = "8.8.8.8" };

            // Act
            var result = await homeController.IntelligenceLookup(model, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult?.Model);

            var viewModel = viewResult!.Model as MX.GeoLocation.Web.Models.IntelligenceLookupViewModel;
            Assert.NotNull(viewModel?.Intelligence);
            Assert.Equal("8.8.8.8", viewModel!.Intelligence!.Address);
        }

        [Fact]
        public async Task IntelligenceLookupPostShouldReturnModelErrorForEmptyAddress()
        {
            // Arrange
            var model = new MX.GeoLocation.Web.Models.IntelligenceLookupViewModel { AddressData = "" };

            // Act
            var result = await homeController.IntelligenceLookup(model, CancellationToken.None);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(homeController.ModelState.IsValid);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task IntelligenceLookupPostShouldHandleApiErrors(HttpStatusCode statusCode)
        {
            // Arrange
            fakeGeoLocationClient.V1_1Lookup.AddIntelligenceErrorResponse("8.8.8.8", statusCode, "ERROR", "Test error");
            var model = new MX.GeoLocation.Web.Models.IntelligenceLookupViewModel { AddressData = "8.8.8.8" };

            // Act
            var result = await homeController.IntelligenceLookup(model, CancellationToken.None);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            var viewModel = viewResult!.Model as MX.GeoLocation.Web.Models.IntelligenceLookupViewModel;
            Assert.Null(viewModel?.Intelligence);
        }

        private static void AssertCityGeoLocationDtoEquals(CityGeoLocationDto expected, CityGeoLocationDto actual)
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
            Assert.Equal(expected.Timezone, actual.Timezone);
        }
    }
}
