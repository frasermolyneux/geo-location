using MX.GeoLocation.LookupApi.Abstractions.Models;
using MX.GeoLocation.PublicWebApp.Extensions;

namespace MX.GeoLocation.PublicWebApp.Tests.Extensions
{
    public class GeoLocationDtoExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestLocationSummaryForCityAndCountry()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto()
            {
                CityName = "London",
                CountryName = "United Kingdom",
                CountryCode = "GB",
                RegisteredCountry = "UK"
            };

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            result.ToString().Should().Be("London, United Kingdom");
        }

        [Test]
        public void TestLocationSummaryForCityAndCountryOnly()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto()
            {
                CityName = "London",
                CountryName = "United Kingdom"
            };

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            result.ToString().Should().Be("London, United Kingdom");
        }

        [Test]
        public void TestLocationSummaryForCountryCodeOnly()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto()
            {
                CountryCode = "GB"
            };

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            result.ToString().Should().Be("GB");
        }

        [Test]
        public void TestLocationSummaryForRegisteredCountryOnly()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto()
            {
                RegisteredCountry = "UK"
            };

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            result.ToString().Should().Be("UK");
        }

        [Test]
        public void TestLocationSummaryForInvalid()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto();

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            result.ToString().Should().Be("Unknown");
        }
    }
}
