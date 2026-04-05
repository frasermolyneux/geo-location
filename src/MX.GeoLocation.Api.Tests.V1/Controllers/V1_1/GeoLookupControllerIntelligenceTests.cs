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
    public class GeoLookupControllerIntelligenceTests
    {
        private readonly Mock<IMaxMindGeoLocationRepository> mockMaxMind;
        private readonly Mock<ITableStorageGeoLocationRepository> mockTableStorage;
        private readonly Mock<IProxyCheckRepository> mockProxyCheck;
        private readonly Mock<IProxyCheckCacheRepository> mockProxyCheckCache;
        private readonly Mock<IIpIntelligenceService> mockIntelligenceService;
        private readonly Mock<IHostnameResolver> mockHostnameResolver;
        private readonly GeoLookupController geoLookupController;

        public GeoLookupControllerIntelligenceTests()
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
            mockHostnameResolver.Setup(x => x.ResolveHostname("1.1.1.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "1.1.1.1"));
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

        // --- GET intelligence ---

        [Fact]
        public async Task GetIntelligence_ReturnsSuccess()
        {
            // Arrange
            var dto = new IpIntelligenceDto
            {
                Address = "8.8.8.8",
                TranslatedAddress = "8.8.8.8",
                CountryName = "United States",
                MaxMindStatus = SourceStatus.Success,
                ProxyCheckStatus = SourceStatus.Success,
                IsPartial = false
            };

            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("8.8.8.8", "8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await geoLookupController.GetIpIntelligence("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);
        }

        [Fact]
        public async Task GetIntelligence_HostnameTranslation_PreservesOriginalHostname()
        {
            // Arrange
            var dto = new IpIntelligenceDto
            {
                Address = "example.com",
                TranslatedAddress = "93.184.216.34",
                CountryName = "United States",
                MaxMindStatus = SourceStatus.Success,
                ProxyCheckStatus = SourceStatus.Success,
                IsPartial = false
            };

            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("example.com", "93.184.216.34", It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await geoLookupController.GetIpIntelligence("example.com", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);

            mockIntelligenceService.Verify(
                x => x.GetIpIntelligence("example.com", "93.184.216.34", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetIntelligence_BothSourcesFail_Returns503()
        {
            // Arrange
            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("8.8.8.8", "8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IpIntelligenceDto?)null);

            // Act
            var result = await geoLookupController.GetIpIntelligence("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(503, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetIntelligence_EmptyHostname_ReturnsBadRequest(string? hostname)
        {
            // Act
            var result = await geoLookupController.GetIpIntelligence(hostname!, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task GetIntelligence_InvalidHostname_ReturnsBadRequest(string invalidHostname)
        {
            // Act
            var result = await geoLookupController.GetIpIntelligence(invalidHostname, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task GetIntelligence_LocalAddress_ReturnsBadRequest(string localhost)
        {
            // Act
            var result = await geoLookupController.GetIpIntelligence(localhost, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task GetIntelligence_ServiceThrows_Returns500()
        {
            // Arrange
            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("8.8.8.8", "8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act
            var result = await geoLookupController.GetIpIntelligence("8.8.8.8", CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult!.StatusCode);
        }

        // --- POST batch intelligence ---

        [Fact]
        public async Task PostBatchIntelligence_ValidHostnames_ReturnsResults()
        {
            // Arrange
            var hostnames = new List<string> { "8.8.8.8", "1.1.1.1" };

            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("8.8.8.8", "8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IpIntelligenceDto
                {
                    Address = "8.8.8.8",
                    TranslatedAddress = "8.8.8.8",
                    MaxMindStatus = SourceStatus.Success,
                    ProxyCheckStatus = SourceStatus.Success
                });
            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("1.1.1.1", "1.1.1.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IpIntelligenceDto
                {
                    Address = "1.1.1.1",
                    TranslatedAddress = "1.1.1.1",
                    MaxMindStatus = SourceStatus.Success,
                    ProxyCheckStatus = SourceStatus.Success
                });

            // Act
            var result = await geoLookupController.GetIpIntelligences(hostnames, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);
        }

        [Fact]
        public async Task PostBatchIntelligence_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await geoLookupController.GetIpIntelligences(null, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task PostBatchIntelligence_EmptyList_ReturnsBadRequest()
        {
            // Act
            var result = await geoLookupController.GetIpIntelligences(new List<string>(), CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task PostBatchIntelligence_OverLimit_ReturnsBadRequest()
        {
            // Arrange
            var hostnames = Enumerable.Range(1, 21).Select(i => $"10.0.0.{i}").ToList();

            // Act
            var result = await geoLookupController.GetIpIntelligences(hostnames, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);
        }

        [Fact]
        public async Task PostBatchIntelligence_AllSourcesFail_Returns200WithErrors()
        {
            // Arrange - service returns null (both sources failed) for the hostname
            var hostnames = new List<string> { "8.8.8.8" };

            mockIntelligenceService
                .Setup(x => x.GetIpIntelligence("8.8.8.8", "8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IpIntelligenceDto?)null);

            // Act
            var result = await geoLookupController.GetIpIntelligences(hostnames, CancellationToken.None);

            // Assert - batch returns 200 with errors aggregated, not 503
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult!.StatusCode);
        }

        // --- DELETE ---

        private static int? GetStatusCode(IActionResult result) => result switch
        {
            ObjectResult obj => obj.StatusCode,
            StatusCodeResult sc => sc.StatusCode,
            _ => null
        };

        [Fact]
        public async Task Delete_Success_ReturnsOk()
        {
            // Arrange
            mockHostnameResolver
                .Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "8.8.8.8"));
            mockTableStorage
                .Setup(x => x.DeleteGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockProxyCheckCache
                .Setup(x => x.DeleteProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await geoLookupController.DeleteMetadata("8.8.8.8", CancellationToken.None);

            // Assert
            Assert.Equal(200, GetStatusCode(result));
        }

        [Fact]
        public async Task Delete_NotFound_Returns404()
        {
            // Arrange
            mockHostnameResolver
                .Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "8.8.8.8"));
            mockTableStorage
                .Setup(x => x.DeleteGeoLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockProxyCheckCache
                .Setup(x => x.DeleteProxyCheckData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await geoLookupController.DeleteMetadata("8.8.8.8", CancellationToken.None);

            // Assert
            Assert.Equal(404, GetStatusCode(result));
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task Delete_LocalAddress_ReturnsBadRequest(string localhost)
        {
            // Act
            var result = await geoLookupController.DeleteMetadata(localhost, CancellationToken.None);

            // Assert
            Assert.Equal(400, GetStatusCode(result));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Delete_EmptyHostname_ReturnsBadRequest(string? hostname)
        {
            // Act
            var result = await geoLookupController.DeleteMetadata(hostname!, CancellationToken.None);

            // Assert
            Assert.Equal(400, GetStatusCode(result));
        }

        [Fact]
        public async Task Delete_HostnameResolvesToDifferentAddress_DeletesBothKeys()
        {
            // Arrange
            mockHostnameResolver
                .Setup(x => x.ResolveHostname("example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "93.184.216.34"));
            mockTableStorage
                .Setup(x => x.DeleteGeoLocation("93.184.216.34", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockTableStorage
                .Setup(x => x.DeleteGeoLocation("example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockProxyCheckCache
                .Setup(x => x.DeleteProxyCheckData("93.184.216.34", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await geoLookupController.DeleteMetadata("example.com", CancellationToken.None);

            // Assert
            Assert.Equal(200, GetStatusCode(result));

            // Verify both addresses were attempted for deletion
            mockTableStorage.Verify(x => x.DeleteGeoLocation("93.184.216.34", It.IsAny<CancellationToken>()), Times.Once);
            mockTableStorage.Verify(x => x.DeleteGeoLocation("example.com", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_HostnameEqualsAddress_DoesNotDeleteHostnameTwice()
        {
            // Arrange
            mockHostnameResolver
                .Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "8.8.8.8"));
            mockTableStorage
                .Setup(x => x.DeleteGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockProxyCheckCache
                .Setup(x => x.DeleteProxyCheckData("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await geoLookupController.DeleteMetadata("8.8.8.8", CancellationToken.None);

            // Assert - DeleteGeoLocation called only once (hostname == address)
            mockTableStorage.Verify(x => x.DeleteGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_HostnameResolutionFails_ReturnsBadRequest()
        {
            // Act
            var result = await geoLookupController.DeleteMetadata("nonexistent.invalid", CancellationToken.None);

            // Assert
            Assert.Equal(400, GetStatusCode(result));
        }

        [Fact]
        public async Task Delete_Exception_Returns500()
        {
            // Arrange
            mockHostnameResolver
                .Setup(x => x.ResolveHostname("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "8.8.8.8"));
            mockTableStorage
                .Setup(x => x.DeleteGeoLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Storage error"));

            // Act
            var result = await geoLookupController.DeleteMetadata("8.8.8.8", CancellationToken.None);

            // Assert
            Assert.Equal(500, GetStatusCode(result));
        }
    }
}
