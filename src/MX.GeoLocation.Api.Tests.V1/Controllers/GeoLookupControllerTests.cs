using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Controllers.V1;
using MX.GeoLocation.LookupWebApi.Repositories;
using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.LookupWebApi.Tests.Controllers
{
    public class GeoLookupControllerTests
    {
        private readonly GeoLookupController geoLookupController;

        public GeoLookupControllerTests()
        {
            var mockHostnameResolver = new Mock<IHostnameResolver>();
            mockHostnameResolver.Setup(x => x.ResolveHostname(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, (string?)null));
            mockHostnameResolver.Setup(x => x.IsLocalAddress(It.IsAny<string>())).Returns(false);
            mockHostnameResolver.Setup(x => x.IsLocalAddress("localhost")).Returns(true);
            mockHostnameResolver.Setup(x => x.IsLocalAddress("127.0.0.1")).Returns(true);
            mockHostnameResolver.Setup(x => x.ResolveHostname("localhost", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "127.0.0.1"));
            mockHostnameResolver.Setup(x => x.ResolveHostname("127.0.0.1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "127.0.0.1"));

            geoLookupController = new GeoLookupController(
                Mock.Of<ILogger<GeoLookupController>>(),
                Mock.Of<ITableStorageGeoLocationRepository>(),
                Mock.Of<IMaxMindGeoLocationRepository>(),
                mockHostnameResolver.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task TestGetGeoLocationReturnsBadRequestForNullOrEmptyHostname(string? hostname)
        {
            // Act
            var result = await geoLookupController.GetGeoLocation(hostname!, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);

            var apiResponseDto = objectResult.Value as ApiResponse<GeoLocationDto>;
            Assert.NotNull(apiResponseDto);
            Assert.NotNull(apiResponseDto!.Errors);
            Assert.NotEmpty(apiResponseDto.Errors!);
            Assert.Equal("The hostname parameter is required and cannot be empty.", apiResponseDto.Errors!.First().Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task TestDeleteMetadataReturnsBadRequestForNullOrEmptyHostname(string? hostname)
        {
            // Act
            var result = await geoLookupController.DeleteMetadata(hostname!, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            var statusCode = result switch
            {
                ObjectResult obj => obj.StatusCode,
                StatusCodeResult sc => sc.StatusCode,
                _ => null
            };

            Assert.Equal(400, statusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task TestGetGeoLocationHandlesInvalidHostname(string invalidHostname)
        {
            // Arrange


            // Act
            var result = await geoLookupController.GetGeoLocation(invalidHostname, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult!.StatusCode);

            Assert.IsType<ApiResponse<GeoLocationDto>>(objectResult.Value);

            var apiResponseDto = objectResult.Value as ApiResponse<GeoLocationDto>;
            Assert.NotNull(apiResponseDto);

            Assert.NotNull(apiResponseDto!.Errors);
            Assert.NotEmpty(apiResponseDto.Errors!);
            Assert.Equal("The address provided is invalid. IP or DNS is acceptable.", apiResponseDto.Errors!.First().Message);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public async Task TestGetGeoLocationHandlesLocalhost(string localhost)
        {
            // Arrange


            // Act
            var result = await geoLookupController.GetGeoLocation(localhost, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult!.StatusCode);

            Assert.IsType<ApiResponse<GeoLocationDto>>(objectResult.Value);

            var apiResponseDto = objectResult.Value as ApiResponse<GeoLocationDto>;
            Assert.NotNull(apiResponseDto);

            Assert.NotNull(apiResponseDto!.Errors);
            Assert.NotEmpty(apiResponseDto.Errors!);
            Assert.Equal("Local addresses are not supported for geo location", apiResponseDto.Errors!.First().Message);
        }

    }
}
