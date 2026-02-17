using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Controllers.V1_1;
using MX.GeoLocation.LookupWebApi.Repositories;

namespace MX.GeoLocation.LookupWebApi.Tests.Controllers.V1_1
{
    public class GeoLookupControllerTests : IDisposable
    {
        private readonly Mock<IMaxMindGeoLocationRepository> mockMaxMind;
        private readonly Mock<ITableStorageGeoLocationRepository> mockTableStorage;
        private readonly GeoLookupController geoLookupController;

        public GeoLookupControllerTests()
        {
            mockMaxMind = new Mock<IMaxMindGeoLocationRepository>();
            mockTableStorage = new Mock<ITableStorageGeoLocationRepository>();

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
                config);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task TestGetCityGeoLocationHandlesInvalidHostname(string invalidHostname)
        {
            // Act
            var result = await geoLookupController.GetCityGeoLocationAction(invalidHostname);

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
            var result = await geoLookupController.GetInsightsGeoLocationAction(invalidHostname);

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
            var result = await geoLookupController.GetCityGeoLocationAction(localhost);

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
            var result = await geoLookupController.GetInsightsGeoLocationAction(localhost);

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
                .Setup(x => x.GetCityGeoLocation("8.8.8.8"))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await geoLookupController.GetCityGeoLocationAction("8.8.8.8");

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind was NOT called
            mockMaxMind.Verify(x => x.GetCityGeoLocation(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TestGetCityGeoLocation_CacheMiss_QueriesMaxMindAndStores()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetCityGeoLocation("8.8.8.8"))
                .ReturnsAsync((CityGeoLocationDto?)null);

            var maxMindDto = new CityGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                CityName = "Mountain View"
            };

            mockMaxMind
                .Setup(x => x.GetCityGeoLocation("8.8.8.8"))
                .ReturnsAsync(maxMindDto);

            // Act
            var result = await geoLookupController.GetCityGeoLocationAction("8.8.8.8");

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind WAS called
            mockMaxMind.Verify(x => x.GetCityGeoLocation("8.8.8.8"), Times.Once);
            // Verify result was stored
            mockTableStorage.Verify(x => x.StoreCityGeoLocation(It.IsAny<CityGeoLocationDto>()), Times.Once);
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
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>()))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await geoLookupController.GetInsightsGeoLocationAction("8.8.8.8");

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind was NOT called
            mockMaxMind.Verify(x => x.GetInsightsGeoLocation(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TestGetInsightsGeoLocation_CacheMiss_QueriesMaxMindAndStores()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>()))
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
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8"))
                .ReturnsAsync(maxMindDto);

            // Act
            var result = await geoLookupController.GetInsightsGeoLocationAction("8.8.8.8");

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify MaxMind WAS called
            mockMaxMind.Verify(x => x.GetInsightsGeoLocation("8.8.8.8"), Times.Once);
            // Verify result was stored
            mockTableStorage.Verify(x => x.StoreInsightsGeoLocation(It.IsAny<InsightsGeoLocationDto>()), Times.Once);
        }

        public void Dispose()
        {
            geoLookupController?.Dispose();
        }
    }
}
