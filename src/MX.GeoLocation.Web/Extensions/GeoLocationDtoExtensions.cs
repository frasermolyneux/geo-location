using System.Net;

using Microsoft.AspNetCore.Html;

using MX.GeoLocation.Abstractions.Models.V1;

namespace MX.GeoLocation.Web.Extensions;

public static class GeoLocationDtoExtensions
{
    public static HtmlString FlagImage(this GeoLocationDto geoLocationDto)
    {
        if (!string.IsNullOrWhiteSpace(geoLocationDto.CountryCode))
        {
            var code = WebUtility.HtmlEncode(geoLocationDto.CountryCode);
            return new HtmlString($"<img src=\"/images/flags/{WebUtility.HtmlEncode(geoLocationDto.CountryCode.ToLower())}.png\" class=\"result-flag\" alt=\"{code}\" />");
        }

        return new HtmlString("<img src=\"/images/flags/unknown.png\" class=\"result-flag\" alt=\"Unknown\" />");
    }

    public static HtmlString LocationSummary(this GeoLocationDto geoLocationDto)
    {
        return !string.IsNullOrWhiteSpace(geoLocationDto.CityName) &&
            !string.IsNullOrWhiteSpace(geoLocationDto.CountryName)
            ? new HtmlString($"{WebUtility.HtmlEncode(geoLocationDto.CityName)}, {WebUtility.HtmlEncode(geoLocationDto.CountryName)}")
            : !string.IsNullOrWhiteSpace(geoLocationDto.CountryCode)
            ? new HtmlString($"{WebUtility.HtmlEncode(geoLocationDto.CountryCode)}")
            : !string.IsNullOrWhiteSpace(geoLocationDto.RegisteredCountry)
            ? new HtmlString($"{WebUtility.HtmlEncode(geoLocationDto.RegisteredCountry)}")
            : new HtmlString("Unknown");
    }
}