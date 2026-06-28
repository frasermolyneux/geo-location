using System.Net;
using System.Net.Sockets;

namespace MX.GeoLocation.LookupWebApi.Services;

public interface IHostnameResolver
{
    Task<(bool Success, string? ResolvedAddress)> ResolveHostname(string hostname, CancellationToken cancellationToken);
    bool IsLocalAddress(string hostname);
    bool IsPrivateOrReservedAddress(string ipAddress);
}

public class HostnameResolver : IHostnameResolver
{
    private static readonly string[] LocalHostnames = ["localhost"];

    public bool IsLocalAddress(string hostname)
    {
        return LocalHostnames.Contains(hostname, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsPrivateOrReservedAddress(string ipAddress)
    {
        var normalized = AddressNormalizer.NormalizeIpLiteral(ipAddress);
        if (normalized is null || !IPAddress.TryParse(normalized, out var ip))
        {
            return false;
        }

        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        if (ip.Equals(IPAddress.IPv6Loopback) || ip.Equals(IPAddress.IPv6None))
        {
            return true;
        }

        var bytes = ip.GetAddressBytes();

        if (ip.AddressFamily == AddressFamily.InterNetwork && bytes.Length == 4)
        {
            // 10.0.0.0/8
            if (bytes[0] == 10)
            {
                return true;
            }
            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                return true;
            }
            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }
            // 127.0.0.0/8 (loopback)
            if (bytes[0] == 127)
            {
                return true;
            }
            // 169.254.0.0/16 (link-local)
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                return true;
            }
            // 0.0.0.0/8
            if (bytes[0] == 0)
            {
                return true;
            }
            // 100.64.0.0/10 (carrier-grade NAT)
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
            {
                return true;
            }
            // 192.0.0.0/24 (IETF protocol assignments)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0)
            {
                return true;
            }
            // 192.0.2.0/24 (TEST-NET-1)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2)
            {
                return true;
            }
            // 198.51.100.0/24 (TEST-NET-2)
            if (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
            {
                return true;
            }
            // 203.0.113.0/24 (TEST-NET-3)
            if (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)
            {
                return true;
            }
            // 224.0.0.0/4 (multicast)
            if (bytes[0] is >= 224 and <= 239)
            {
                return true;
            }
            // 240.0.0.0/4 (reserved)
            if (bytes[0] >= 240)
            {
                return true;
            }
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv4MappedToIPv6)
            {
                return IsPrivateOrReservedAddress(ip.MapToIPv4().ToString());
            }

            // fe80::/10 (link-local)
            if (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80)
            {
                return true;
            }
            // fc00::/7 (unique local)
            if ((bytes[0] & 0xfe) == 0xfc)
            {
                return true;
            }
            // ff00::/8 (multicast)
            if (bytes[0] == 0xff)
            {
                return true;
            }
            // 2001:db8::/32 (documentation prefix)
            if (bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8)
            {
                return true;
            }
            // ::1 (loopback) and :: (unspecified)
            if (ip.Equals(IPAddress.IPv6Loopback) || ip.Equals(IPAddress.IPv6Any))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<(bool Success, string? ResolvedAddress)> ResolveHostname(string hostname, CancellationToken cancellationToken)
    {
        var normalizedInput = AddressNormalizer.NormalizeIpLiteral(hostname);
        if (normalizedInput is not null)
        {
            return (true, normalizedInput);
        }

        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(hostname, cancellationToken);
            // Prefer globally routable addresses where possible.
            var addr = hostEntry.AddressList.FirstOrDefault(a => !IsPrivateOrReservedAddress(a.ToString()))
                       ?? hostEntry.AddressList.FirstOrDefault();
            if (addr is not null)
            {
                return (true, addr.ToString());
            }
        }
        catch (SocketException)
        {
            return (false, null);
        }

        return (false, null);
    }
}
