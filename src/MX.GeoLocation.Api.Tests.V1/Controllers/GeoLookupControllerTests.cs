using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Controllers;
using MX.GeoLocation.LookupWebApi.Repositories;

namespace MX.GeoLocation.LookupWebApi.Tests.Controllers
{
    public class GeoLookupControllerTests
    {
        private GeoLookupController geoLookupController;

        [SetUp]
        public void Setup()
        {
            geoLookupController = new GeoLookupController(A.Fake<ILogger<GeoLookupController>>(), A.Fake<ITableStorageGeoLocationRepository>(), A.Fake<IMaxMindGeoLocationRepository>());
        }

        [TestCase("abcdefg")]
        [TestCase("a.b.c.d")]
        public async Task TestGetGeoLocationHandlesInvalidHostname(string invalidHostname)
        {
            // Arrange


            // Act
            var result = await geoLookupController.GetGeoLocation(invalidHostname);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(400));

            Assert.That(objectResult.Value, Is.InstanceOf<ApiResponse<GeoLocationDto>>());

            var apiResponseDto = objectResult.Value as ApiResponse<GeoLocationDto>;
            Assert.That(apiResponseDto, Is.Not.Null);

            Assert.That(apiResponseDto!.Errors, Is.Not.Null.And.Not.Empty);
            Assert.That(apiResponseDto.Errors!.First().Message, Is.EqualTo("The address provided is invalid. IP or DNS is acceptable."));
        }

        [TestCase("localhost")]
        [TestCase("127.0.0.1")]
        public async Task TestGetGeoLocationHandlesLocalhost(string localhost)
        {
            // Arrange


            // Act
            var result = await geoLookupController.GetGeoLocation(localhost);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            Assert.That(objectResult.Value, Is.InstanceOf<ApiResponse<GeoLocationDto>>());

            var apiResponseDto = objectResult.Value as ApiResponse<GeoLocationDto>;
            Assert.That(apiResponseDto, Is.Not.Null);

            //apiResponseDto?.Errors.Should().NotBeNullOrEmpty();
            //apiResponseDto?.Errors.First().Message.Should().Be("Hostname is a loopback or local address, geo location data is unavailable");
        }

        [TearDown]
        public void Cleanup()
        {
            geoLookupController?.Dispose();
        }
    }
}
