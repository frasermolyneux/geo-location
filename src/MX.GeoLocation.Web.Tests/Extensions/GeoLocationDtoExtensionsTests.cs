using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Web.Extensions;

namespace MX.GeoLocation.Web.Tests.Extensions
{
    public class GeoLocationDtoExtensionsTests
    {
        [Fact]
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
            Assert.Equal("London, United Kingdom", result.ToString());
        }

        [Fact]
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
            Assert.Equal("London, United Kingdom", result.ToString());
        }

        [Fact]
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
            Assert.Equal("GB", result.ToString());
        }

        [Fact]
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
            Assert.Equal("UK", result.ToString());
        }

        [Fact]
        public void LocationSummaryShouldBeUnknownWhenNoPropertiesAreSet()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto();

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            Assert.Equal("Unknown", result.ToString());
        }
    }
}
