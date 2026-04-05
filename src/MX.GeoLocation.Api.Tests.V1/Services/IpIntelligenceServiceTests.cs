using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MX.GeoLocation.Abstractions.Models.V1_1;
using MX.GeoLocation.LookupWebApi.Repositories;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.Api.Tests.V1.Services
{
    [Trait("Category", "Unit")]
    public class IpIntelligenceServiceTests
    {
        private readonly Mock<IMaxMindGeoLocationRepository> mockMaxMind;
        private readonly Mock<ITableStorageGeoLocationRepository> mockTableStorage;
        private readonly Mock<IProxyCheckRepository> mockProxyCheck;
        private readonly Mock<IProxyCheckCacheRepository> mockProxyCheckCache;
        private readonly IpIntelligenceService sut;

        public IpIntelligenceServiceTests()
        {
            mockMaxMind = new Mock<IMaxMindGeoLocationRepository>();
            mockTableStorage = new Mock<ITableStorageGeoLocationRepository>();
            mockProxyCheck = new Mock<IProxyCheckRepository>();
            mockProxyCheckCache = new Mock<IProxyCheckCacheRepository>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Caching:InsightsCacheDays", "7" },
                    { "Caching:ProxyCheckCacheMinutes", "60" }
                })
                .Build();

            sut = new IpIntelligenceService(
                mockMaxMind.Object,
                mockTableStorage.Object,
                mockProxyCheck.Object,
                mockProxyCheckCache.Object,
                config,
                Mock.Of<ILogger<IpIntelligenceService>>());
        }

        [Fact]
        public async Task BothSourcesSucceed_ReturnsFullResult()
        {
            // Arrange
            var insights = new InsightsGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                ContinentCode = "NA",
                CityName = "Mountain View",
                Anonymizer = new AnonymizerDto()
            };

            var proxyCheckDto = new ProxyCheckDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                RiskScore = 10,
                IsProxy = false,
                Country = "United States"
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);
            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(insights);
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(proxyCheckDto);

            // Act
            var result = await sut.GetIpIntelligence("example.com", "8.8.8.8", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsPartial);
            Assert.Equal(SourceStatus.Success, result.MaxMindStatus);
            Assert.Equal(SourceStatus.Success, result.ProxyCheckStatus);
            Assert.Equal("example.com", result.Address);
            Assert.Equal("8.8.8.8", result.TranslatedAddress);
            Assert.Equal("United States", result.CountryName);
            Assert.Equal("NA", result.ContinentCode);
            Assert.NotNull(result.ProxyCheck);
            Assert.Equal(10, result.ProxyCheck!.RiskScore);
        }

        [Fact]
        public async Task ProxyCheckFails_MaxMindSucceeds_ReturnsPartialResult()
        {
            // Arrange
            var insights = new InsightsGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                Anonymizer = new AnonymizerDto()
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);
            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(insights);
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("ProxyCheck API error"));

            // Act
            var result = await sut.GetIpIntelligence("8.8.8.8", "8.8.8.8", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsPartial);
            Assert.Equal(SourceStatus.Success, result.MaxMindStatus);
            Assert.Equal(SourceStatus.Failed, result.ProxyCheckStatus);
            Assert.Equal("United States", result.CountryName);
            Assert.Null(result.ProxyCheck);
        }

        [Fact]
        public async Task MaxMindFails_ProxyCheckSucceeds_ReturnsPartialResult()
        {
            // Arrange
            var proxyCheckDto = new ProxyCheckDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                RiskScore = 50,
                IsProxy = true
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);
            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("MaxMind error"));
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(proxyCheckDto);

            // Act
            var result = await sut.GetIpIntelligence("8.8.8.8", "8.8.8.8", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsPartial);
            Assert.Equal(SourceStatus.Failed, result.MaxMindStatus);
            Assert.Equal(SourceStatus.Success, result.ProxyCheckStatus);
            Assert.Null(result.CountryName);
            Assert.Null(result.Anonymizer);
            Assert.NotNull(result.ProxyCheck);
            Assert.Equal(50, result.ProxyCheck!.RiskScore);
            Assert.True(result.ProxyCheck.IsProxy);
        }

        [Fact]
        public async Task BothFail_ReturnsNull()
        {
            // Arrange
            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);
            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("MaxMind error"));
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("ProxyCheck error"));

            // Act
            var result = await sut.GetIpIntelligence("8.8.8.8", "8.8.8.8", CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CacheHit_BothSources_NoLiveCalls()
        {
            // Arrange
            var cachedInsights = new InsightsGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                Anonymizer = new AnonymizerDto()
            };

            var cachedProxyCheck = new ProxyCheckDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                RiskScore = 5
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedInsights);
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedProxyCheck);

            // Act
            var result = await sut.GetIpIntelligence("8.8.8.8", "8.8.8.8", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsPartial);
            mockMaxMind.Verify(x => x.GetInsightsGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            mockProxyCheck.Verify(x => x.GetProxyCheckData(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProxyCheckFailure_DoesNotCacheResult()
        {
            // Arrange
            var insights = new InsightsGeoLocationDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                Anonymizer = new AnonymizerDto()
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);
            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(insights);
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("API error"));

            // Act
            await sut.GetIpIntelligence("8.8.8.8", "8.8.8.8", CancellationToken.None);

            // Assert
            mockProxyCheckCache.Verify(
                x => x.StoreProxyCheckData(It.IsAny<ProxyCheckDto>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task MaxMindFailure_DoesNotCacheInsights()
        {
            // Arrange
            var proxyCheckDto = new ProxyCheckDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                RiskScore = 10
            };

            mockTableStorage
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InsightsGeoLocationDto?)null);
            mockMaxMind
                .Setup(x => x.GetInsightsGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("MaxMind error"));
            mockProxyCheckCache
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProxyCheckDto?)null);
            mockProxyCheck
                .Setup(x => x.GetProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(proxyCheckDto);

            // Act
            await sut.GetIpIntelligence("8.8.8.8", "8.8.8.8", CancellationToken.None);

            // Assert
            mockTableStorage.Verify(
                x => x.StoreInsightsGeoLocation(It.IsAny<InsightsGeoLocationDto>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
