using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Controllers.V1;
using MX.GeoLocation.LookupWebApi.Repositories;

namespace MX.GeoLocation.LookupWebApi.Tests.Controllers
{
    public class GeoLookupControllerTests : IDisposable
    {
        private readonly GeoLookupController geoLookupController;

        public GeoLookupControllerTests()
        {
            geoLookupController = new GeoLookupController(Mock.Of<ILogger<GeoLookupController>>(), Mock.Of<ITableStorageGeoLocationRepository>(), Mock.Of<IMaxMindGeoLocationRepository>());
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("a.b.c.d")]
        public async Task TestGetGeoLocationHandlesInvalidHostname(string invalidHostname)
        {
            // Arrange


            // Act
            var result = await geoLookupController.GetGeoLocation(invalidHostname);

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
            var result = await geoLookupController.GetGeoLocation(localhost);

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

        public void Dispose()
        {
            geoLookupController?.Dispose();
        }
    }
}
