using System.Net;

using Microsoft.AspNetCore.Html;

using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.Web.Extensions
{
    public static class IpIntelligenceExtensions
    {
        public static HtmlString FlagImage(this IpIntelligenceDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.CountryCode))
            {
                var code = WebUtility.HtmlEncode(dto.CountryCode);
                return new HtmlString($"<img src=\"/images/flags/{WebUtility.HtmlEncode(dto.CountryCode.ToLower())}.png\" class=\"result-flag\" alt=\"{code}\" />");
            }

            return new HtmlString("<img src=\"/images/flags/unknown.png\" class=\"result-flag\" alt=\"Unknown\" />");
        }

        public static HtmlString LocationSummary(this IpIntelligenceDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.CityName) && !string.IsNullOrWhiteSpace(dto.CountryName))
                return new HtmlString($"{WebUtility.HtmlEncode(dto.CityName)}, {WebUtility.HtmlEncode(dto.CountryName)}");

            if (!string.IsNullOrWhiteSpace(dto.CountryCode))
                return new HtmlString($"{WebUtility.HtmlEncode(dto.CountryCode)}");

            return new HtmlString("Unknown");
        }

        public static string RiskBadgeClass(this ProxyCheckDto? proxyCheck)
        {
            if (proxyCheck is null) return "text-bg-secondary";

            return proxyCheck.RiskScore switch
            {
                >= 80 => "text-bg-danger",
                >= 50 => "text-bg-warning",
                >= 25 => "text-bg-info",
                _ => "text-bg-success"
            };
        }

        public static string RiskLabel(this ProxyCheckDto? proxyCheck)
        {
            if (proxyCheck is null) return "N/A";

            return proxyCheck.RiskScore switch
            {
                >= 80 => "High Risk",
                >= 50 => "Medium-High",
                >= 25 => "Medium-Low",
                _ => "Low Risk"
            };
        }

        public static string SourceStatusBadgeClass(this SourceStatus status) => status switch
        {
            SourceStatus.Success => "text-bg-success",
            SourceStatus.Failed => "text-bg-danger",
            SourceStatus.Unavailable => "text-bg-secondary",
            _ => "text-bg-secondary"
        };

        public static HtmlString FlagImage(this CityGeoLocationDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.CountryCode))
            {
                var code = WebUtility.HtmlEncode(dto.CountryCode);
                return new HtmlString($"<img src=\"/images/flags/{WebUtility.HtmlEncode(dto.CountryCode.ToLower())}.png\" class=\"result-flag\" alt=\"{code}\" />");
            }

            return new HtmlString("<img src=\"/images/flags/unknown.png\" class=\"result-flag\" alt=\"Unknown\" />");
        }

        public static HtmlString LocationSummary(this CityGeoLocationDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.CityName) && !string.IsNullOrWhiteSpace(dto.CountryName))
                return new HtmlString($"{WebUtility.HtmlEncode(dto.CityName)}, {WebUtility.HtmlEncode(dto.CountryName)}");

            if (!string.IsNullOrWhiteSpace(dto.CountryCode))
                return new HtmlString($"{WebUtility.HtmlEncode(dto.CountryCode)}");

            return new HtmlString("Unknown");
        }
    }
}
