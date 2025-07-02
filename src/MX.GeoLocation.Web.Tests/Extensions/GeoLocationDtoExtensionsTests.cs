using MX.GeoLocation.LookupApi.Abstractions.Models;
using MX.GeoLocation.PublicWebApp.Extensions;

namespace MX.GeoLocation.PublicWebApp.Tests.Extensions
{
    internal class GeoLocationDtoExtensionsTests
    {
        [Test]
        public void LocationSummaryShouldBeCityAndCountryWhenAllPropertiesPresent()
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
        public void LocationSummaryShouldBeCityAndCountryWhenCityAndCountryOnly()
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
        public void LocationSummaryShouldBeCountryCodeWhenCountryCodeOnly()
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
        public void LocationSummaryShouldBeRegisteredCountryWhenRegisteredCountryOnly()
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
        public void LocationSummaryShouldBeUnknownWhenNoPropertiesAreSet()
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
