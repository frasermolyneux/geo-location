using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Controllers.V1_1;
using MX.GeoLocation.LookupWebApi.Repositories;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.Api.Tests.V1.Controllers.V1_1
{
    [Trait("Category", "Unit")]
    public class GeoLookupControllerProxyCheckTests
    {
        private readonly Mock<IMaxMindGeoLocationRepository> mockMaxMind;
        private readonly Mock<ITableStorageGeoLocationRepository> mockTableStorage;
        private readonly Mock<IProxyCheckRepository> mockProxyCheck;
        private readonly Mock<IProxyCheckCacheRepository> mockProxyCheckCache;
        private readonly Mock<IIpIntelligenceService> mockIntelligenceService;
        private readonly Mock<IHostnameResolver> mockHostnameResolver;
        private readonly GeoLookupController geoLookupController;

        public GeoLookupControllerProxyCheckTests()
        {
            mockMaxMind = new Mock<IMaxMindGeoLocationRepository>();
            mockTableStorage = new Mock<ITableStorageGeoLocationRepository>();
            mockProxyCheck = new Mock<IProxyCheckRepository>();
            mockProxyCheckCache = new Mock<IProxyCheckCacheRepository>();
            mockIntelligenceService = new Mock<IIpIntelligenceService>();
            mockHostnameResolver = new Mock<IHostnameResolver>();

            mockHostnameResolver.Setup(x => x.ResolveHostname(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, (string?)null));
            mockHostnameResolver.Setup(x => x.IsLocalAddress(It.IsAny<string>())).Returns(false);
            mockHostnameResolver.Setup(x => x.IsLocalAddress("localhost")).Returns(true);
            mockHostnameResolver.Setup(x => x.IsLocalAddress("127.0.0.1")).Returns(true);
            mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress(It.IsAny<string>())).Returns(false);
            mockHostnameResolver.Setup(x => x.IsPrivateOrReservedAddress("127.0.0.1")).Returns(true);
            mockHostnameResolver.Setup(x => x.ResolveHostname("localhost", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "127.0.0.1"));
            mockHostnameResolver.Setup(x => x.ResolveHostname("127.0.0.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "127.0.0.1"));
            mockHostnameResolver.Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "8.8.8.8"));
            mockHostnameResolver.Setup(x => x.ResolveHostname("example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "93.184.216.34"));

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Caching:InsightsCacheDays", "7" },
                    { "Caching:ProxyCheckCacheMinutes", "60" }
                })
                .Build();

            var geoLookupService = new GeoLookupService(
                Mock.Of<ILogger<GeoLookupService>>(),
                mockHostnameResolver.Object);

            geoLookupController = new GeoLookupController(
                mockMaxMind.Object,
                mockTableStorage.Object,
                mockProxyCheck.Object,
                mockProxyCheckCache.Object,
                geoLookupService,
                mockIntelligenceService.Object,
                mockHostnameResolver.Object,
                config,
                Mock.Of<ILogger<GeoLookupController>>());
        }

        [Fact]
        public async Task GetProxyCheck_CacheHit_ReturnsCachedData()
        {
            // Arrange
            var cachedDto = new ProxyCheckDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                RiskScore = 10,
                IsProxy = false,
                Country = "United States"
            };

            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await geoLookupController.GetProxyCheck("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify live repository was NOT called
            mockProxyCheck.Verify(x => x.GetProxyCheckData(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetProxyCheck_CacheMiss_CallsRepositoryAndStores()
        {
            // Arrange
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);

            var liveDto = new ProxyCheckDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                RiskScore = 25,
                IsProxy = false,
                Country = "United States"
            };

            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(liveDto);

            // Act
            var result = await geoLookupController.GetProxyCheck("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify live repository WAS called
            mockProxyCheck.Verify(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()), Times.Once);
            // Verify result was stored in cache
            mockProxyCheckCache.Verify(x => x.StoreProxyCheckData(It.IsAny<ProxyCheckDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetProxyCheck_HostnameTranslation_PreservesOriginalHostname()
        {
            // Arrange
            var liveDto = new ProxyCheckDto
            {
                Address = "93.184.216.34",
                TranslatedAddress = "93.184.216.34",
                RiskScore = 5,
                IsProxy = false
            };

            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("93.184.216.34", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("93.184.216.34", It.IsAny<CancellationToken>()))
                .ReturnsAsync(liveDto);

            // Act
            var result = await geoLookupController.GetProxyCheck("example.com", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            // Verify the resolved address was used for the lookup
            mockProxyCheck.Verify(x => x.GetProxyCheckData("93.184.216.34", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProxyCheck_EmptyHostname_ReturnsBadRequest(string? hostname)
        {
            // Act
            var result = await geoLookupController.GetProxyCheck(hostname!, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task GetProxyCheck_LocalAddress_ReturnsBadRequest(string localhost)
        {
            // Act
            var result = await geoLookupController.GetProxyCheck(localhost, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task GetProxyCheck_InvalidHostname_ReturnsBadRequest(string invalidHostname)
        {
            // Act
            var result = await geoLookupController.GetProxyCheck(invalidHostname, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task GetProxyCheck_RepositoryThrows_Returns500()
        {
            // Arrange
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("ProxyCheck API error"));

            // Act
            var result = await geoLookupController.GetProxyCheck("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult!.StatusCode);
        }
    }
}
