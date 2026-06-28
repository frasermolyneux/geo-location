using System.Net;

namespace MX.GeoLocation.LookupWebApi.Services;

/// <summary>
/// Normalizes host/address input into a deterministic form for lookup, cache keying, and provider calls.
/// </summary>
public static class AddressNormalizer
{
    /// <summary>
    /// Returns the canonical textual representation of an IP address input.
    /// Supports optional IPv6 brackets and scope identifiers (zone ids).
    /// Returns null when input is not a valid IP literal.
    /// </summary>
    public static string? NormalizeIpLiteral(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var candidate = input.Trim();

        if (candidate.Length >= 2 && candidate[0] == '[' && candidate[^1] == ']')
        {
            candidate = candidate[1..^1];
        }

        // Scope identifiers (for example fe80::1%eth0) are not useful for API/cache identity.
        var scopeSeparator = candidate.IndexOf('%');
        if (scopeSeparator >= 0)
        {
            candidate = candidate[..scopeSeparator];
        }

        return IPAddress.TryParse(candidate, out var parsed) ? parsed.ToString() : null;
    }

    /// <summary>
    /// Returns true when input is a valid IP literal (after normalization rules).
    /// </summary>
    public static bool IsIpLiteral(string input)
    {
        return NormalizeIpLiteral(input) is not null;
    }
}