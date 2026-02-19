using System.Net;
using System.Net.Sockets;

namespace MX.GeoLocation.LookupWebApi.Services;

public interface IHostnameResolver
{
    Task<(bool Success, string? ResolvedAddress)> ResolveHostname(string hostname, CancellationToken cancellationToken);
    bool IsLocalAddress(string hostname);
}

public class HostnameResolver : IHostnameResolver
{
    private static readonly string[] LocalOverrides = ["localhost", "127.0.0.1"];

    public bool IsLocalAddress(string hostname)
    {
        return LocalOverrides.Contains(hostname, StringComparer.OrdinalIgnoreCase);
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
            if (hostEntry.AddressList.FirstOrDefault() is not null)
            {
                return (true, hostEntry.AddressList.First().ToString());
            }
        }
        catch (SocketException)
        {
            return (false, null);
        }

        return (false, null);
    }
}
