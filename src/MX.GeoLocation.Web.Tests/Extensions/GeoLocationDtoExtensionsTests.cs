using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Web.Extensions;

namespace MX.GeoLocation.Web.Tests.Extensions
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
            Assert.That(result.ToString(), Is.EqualTo("London, United Kingdom"));
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
            Assert.That(result.ToString(), Is.EqualTo("London, United Kingdom"));
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
            Assert.That(result.ToString(), Is.EqualTo("GB"));
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
            Assert.That(result.ToString(), Is.EqualTo("UK"));
        }

        [Test]
        public void LocationSummaryShouldBeUnknownWhenNoPropertiesAreSet()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto();

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            Assert.That(result.ToString(), Is.EqualTo("Unknown"));
        }
    }
}
