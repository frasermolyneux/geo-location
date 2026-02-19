using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Controllers.V1_1;
using MX.GeoLocation.LookupWebApi.Repositories;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.LookupWebApi.Tests.Controllers.V1_1
{
    public class GeoLookupControllerTests
    {
        private readonly Mock<IMaxMindGeoLocationRepository> mockMaxMind;
        private readonly Mock<ITableStorageGeoLocationRepository> mockTableStorage;
        private readonly Mock<IHostnameResolver> mockHostnameResolver;
        private readonly GeoLookupController geoLookupController;

        public GeoLookupControllerTests()
        {
            mockMaxMind = new Mock<IMaxMindGeoLocationRepository>();
            mockTableStorage = new Mock<ITableStorageGeoLocationRepository>();
            mockHostnameResolver = new Mock<IHostnameResolver>();

            mockHostnameResolver.Setup(x => x.ResolveHostname(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, (string?)null));
            mockHostnameResolver.Setup(x => x.IsLocalAddress(It.IsAny<string>())).Returns(false);
            mockHostnameResolver.Setup(x => x.IsLocalAddress("localhost")).Returns(true);
            mockHostnameResolver.Setup(x => x.IsLocalAddress("127.0.0.1")).Returns(true);
            mockHostnameResolver.Setup(x => x.ResolveHostname("localhost", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "127.0.0.1"));
            mockHostnameResolver.Setup(x => x.ResolveHostname("127.0.0.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "127.0.0.1"));
            mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "8.8.8.8"));

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Caching:InsightsCacheDays", "7" }
                })
                .Build();

            geoLookupController = new GeoLookupController(
                Mock.Of<ILogger<GeoLookupController>>(),
                mockMaxMind.Object,
                mockTableStorage.Object,
                mockHostnameResolver.Object,
                config);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task TestGetCityGeoLocationReturnsBadRequestForNullOrEmptyHostname(string? hostname)
        {
            // Act
            var result = await geoLookupController.GetCityGeoLocation(hostname!, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task TestGetInsightsGeoLocationReturnsBadRequestForNullOrEmptyHostname(string? hostname)
        {
            // Act
            var result = await geoLookupController.GetInsightsGeoLocation(hostname!, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task TestGetCityGeoLocationHandlesInvalidHostname(string invalidHostname)
        {
            // Act
            var result = await geoLookupController.GetCityGeoLocation(invalidHostname, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task TestGetInsightsGeoLocationHandlesInvalidHostname(string invalidHostname)
        {
            // Act
            var result = await geoLookupController.GetInsightsGeoLocation(invalidHostname, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task TestGetCityGeoLocationHandlesLocalhost(string localhost)
        {
            // Act
            var result = await geoLookupController.GetCityGeoLocation(localhost, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task TestGetInsightsGeoLocationHandlesLocalhost(string localhost)
        {
            // Act
            var result = await geoLookupController.GetInsightsGeoLocation(localhost, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task TestGetCityGeoLocation_CacheHit_ReturnsCachedData()
        {
            // Arrange
            var cachedDto = new CityGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                CityName = "Mountain View"
            };

            mockTableStorage
                .Setup(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await geoLookupController.GetCityGeoLocation("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind was NOT called
            mockMaxMind.Verify(x => x.GetCityGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TestGetCityGeoLocation_CacheMiss_QueriesMaxMindAndStores()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((CityGeoLocationDto?)null);

            var maxMindDto = new CityGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                CityName = "Mountain View"
            };

            mockMaxMind
                .Setup(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(maxMindDto);

            // Act
            var result = await geoLookupController.GetCityGeoLocation("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind WAS called
            mockMaxMind.Verify(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()), Times.Once);
            // Verify result was stored
            mockTableStorage.Verify(x => x.StoreCityGeoLocation(It.IsAny<CityGeoLocationDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetInsightsGeoLocation_CacheHit_ReturnsCachedData()
        {
            // Arrange
            var cachedDto = new InsightsGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                CityName = "Mountain View",
                Anonymizer = new AnonymizerDto()
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await geoLookupController.GetInsightsGeoLocation("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind was NOT called
            mockMaxMind.Verify(x => x.GetInsightsGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TestGetInsightsGeoLocation_CacheMiss_QueriesMaxMindAndStores()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);

            var maxMindDto = new InsightsGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                CityName = "Mountain View",
                Anonymizer = new AnonymizerDto()
            };

            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(maxMindDto);

            // Act
            var result = await geoLookupController.GetInsightsGeoLocation("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind WAS called
            mockMaxMind.Verify(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()), Times.Once);
            // Verify result was stored
            mockTableStorage.Verify(x => x.StoreInsightsGeoLocation(It.IsAny<InsightsGeoLocationDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCityGeoLocation_AddressNotFound_ReturnsNotFound()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetCityGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((CityGeoLocationDto?)null);

            mockMaxMind
                .Setup(x => x.GetCityGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MaxMind.GeoIP2.Exceptions.AddressNotFoundException("Address not found"));

            mockHostnameResolver
                .Setup(x => x.ResolveHostname("198.51.100.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "198.51.100.1"));

            // Act
            var result = await geoLookupController.GetCityGeoLocation("198.51.100.1", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(404, objectResult!.StatusCode);
        }

        [Fact]
        public async Task TestGetCityGeoLocation_GeoIP2Exception_ReturnsBadRequest()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetCityGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((CityGeoLocationDto?)null);

            mockMaxMind
                .Setup(x => x.GetCityGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MaxMind.GeoIP2.Exceptions.GeoIP2Exception("Service error"));

            mockHostnameResolver
                .Setup(x => x.ResolveHostname("203.0.113.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "203.0.113.1"));

            // Act
            var result = await geoLookupController.GetCityGeoLocation("203.0.113.1", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task TestGetCityGeoLocation_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((CityGeoLocationDto?)null);

            mockMaxMind
                .Setup(x => x.GetCityGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected"));

            // Act
            var result = await geoLookupController.GetCityGeoLocation("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult!.StatusCode);
        }

        [Fact]
        public async Task TestGetInsightsGeoLocation_AddressNotFound_ReturnsNotFound()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("198.51.100.1", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);

            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("198.51.100.1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MaxMind.GeoIP2.Exceptions.AddressNotFoundException("Address not found"));

            mockHostnameResolver
                .Setup(x => x.ResolveHostname("198.51.100.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "198.51.100.1"));

            // Act
            var result = await geoLookupController.GetInsightsGeoLocation("198.51.100.1", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(404, objectResult!.StatusCode);
        }

        [Fact]
        public async Task TestGetInsightsGeoLocation_GeoIP2Exception_ReturnsBadRequest()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("203.0.113.1", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);

            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("203.0.113.1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MaxMind.GeoIP2.Exceptions.GeoIP2Exception("Service error"));

            mockHostnameResolver
                .Setup(x => x.ResolveHostname("203.0.113.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "203.0.113.1"));

            // Act
            var result = await geoLookupController.GetInsightsGeoLocation("203.0.113.1", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task TestGetInsightsGeoLocation_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);

            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected"));

            // Act
            var result = await geoLookupController.GetInsightsGeoLocation("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult!.StatusCode);
        }
    }
}
