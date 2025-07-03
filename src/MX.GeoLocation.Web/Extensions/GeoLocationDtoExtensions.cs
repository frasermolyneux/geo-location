using Microsoft.AspNetCore.Html;

using MX.GeoLocation.Abstractions.Models.V1;

namespace MX.GeoLocation.Web.Extensions
{
    public static class GeoLocationDtoExtensions
    {
        public static HtmlString FlagImage(this GeoLocationDto geoLocationDto)
        {
            return !string.IsNullOrWhiteSpace(geoLocationDto.CountryCode)
                ? new HtmlString($"<img src=\"/images/flags/{geoLocationDto.CountryCode.ToLower()}.png\" class=\"result-flag\" alt=\"{geoLocationDto.CountryCode}\" />")
                : new HtmlString("<img src=\"/images/flags/unknown.png\" class=\"result-flag\" alt=\"Unknown\" />");
        }

        public static HtmlString LocationSummary(this GeoLocationDto geoLocationDto)
        {
            if (!string.IsNullOrWhiteSpace(geoLocationDto.CityName) &&
                !string.IsNullOrWhiteSpace(geoLocationDto.CountryName))
                return new HtmlString($"{geoLocationDto.CityName}, {geoLocationDto.CountryName}");

            if (!string.IsNullOrWhiteSpace(geoLocationDto.CountryCode))
                return new HtmlString($"{geoLocationDto.CountryCode}");

            if (!string.IsNullOrWhiteSpace(geoLocationDto.RegisteredCountry))
                return new HtmlString($"{geoLocationDto.RegisteredCountry}");

            return new HtmlString("Unknown");
        }
    }
}