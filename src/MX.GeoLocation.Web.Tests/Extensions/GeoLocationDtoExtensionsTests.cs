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

        [Fact]
        public void LocationSummaryShouldEncodeHtmlInCityAndCountry()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto()
            {
                CityName = "<script>alert('xss')</script>",
                CountryName = "<img onerror=alert(1) src=x>"
            };

            // Act
            var result = geoLocationDto.LocationSummary();

            // Assert
            Assert.DoesNotContain("<script>", result.ToString());
            Assert.Equal("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;, &lt;img onerror=alert(1) src=x&gt;", result.ToString());
        }

        [Fact]
        public void FlagImageShouldEncodeHtmlInCountryCode()
        {
            // Arrange
            var geoLocationDto = new GeoLocationDto()
            {
                CountryCode = "\"><script>alert('xss')</script>"
            };

            // Act
            var result = geoLocationDto.FlagImage();

            // Assert
            Assert.DoesNotContain("<script>", result.ToString());
        }
    }
}
