namespace MX.GeoLocation.LookupWebApi.Services;

internal static class LogSanitizer
{
    public static string Sanitize(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("\r", string.Empty, StringComparison.Ordinal)
                .Replace("\n", string.Empty, StringComparison.Ordinal);
    }
}
