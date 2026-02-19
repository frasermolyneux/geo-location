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
        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.Equals(IPAddress.IPv6Loopback) || ip.Equals(IPAddress.IPv6None))
            return true;

        var bytes = ip.GetAddressBytes();

        if (ip.AddressFamily == AddressFamily.InterNetwork && bytes.Length == 4)
        {
            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;
            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;
            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;
            // 127.0.0.0/8 (loopback)
            if (bytes[0] == 127)
                return true;
            // 169.254.0.0/16 (link-local)
            if (bytes[0] == 169 && bytes[1] == 254)
                return true;
            // 0.0.0.0/8
            if (bytes[0] == 0)
                return true;
            // 100.64.0.0/10 (carrier-grade NAT)
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
                return true;
            // 192.0.0.0/24 (IETF protocol assignments)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0)
                return true;
            // 192.0.2.0/24 (TEST-NET-1)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2)
                return true;
            // 198.51.100.0/24 (TEST-NET-2)
            if (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
                return true;
            // 203.0.113.0/24 (TEST-NET-3)
            if (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)
                return true;
            // 224.0.0.0/4 (multicast)
            if (bytes[0] >= 224 && bytes[0] <= 239)
                return true;
            // 240.0.0.0/4 (reserved)
            if (bytes[0] >= 240)
                return true;
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // fe80::/10 (link-local)
            if (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80)
                return true;
            // fc00::/7 (unique local)
            if ((bytes[0] & 0xfe) == 0xfc)
                return true;
            // ::1 (loopback) and :: (unspecified)
            if (ip.Equals(IPAddress.IPv6Loopback) || ip.Equals(IPAddress.IPv6Any))
                return true;
        }

        return false;
    }

    public async Task<(bool Success, string? ResolvedAddress)> ResolveHostname(string hostname, CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(hostname, out var ipAddress))
        {
            return (true, ipAddress.ToString());
        }

        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(hostname, cancellationToken);
            if (hostEntry.AddressList.FirstOrDefault() is { } addr)
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
