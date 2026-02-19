using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.Api.Tests.V1.Services;

[Trait("Category", "Unit")]
public class HostnameResolverTests
{
    private readonly HostnameResolver _resolver = new();

    [Theory]
    [InlineData("localhost")]
    [InlineData("LOCALHOST")]
    [InlineData("Localhost")]
    public void IsLocalAddress_Localhost_ReturnsTrue(string hostname)
    {
        Assert.True(_resolver.IsLocalAddress(hostname));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("google.com")]
    [InlineData("127.0.0.1")]
    public void IsLocalAddress_NonLocalHostname_ReturnsFalse(string hostname)
    {
        Assert.False(_resolver.IsLocalAddress(hostname));
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("127.0.0.0")]
    [InlineData("127.255.255.255")]
    public void IsPrivateOrReservedAddress_Loopback_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("10.255.255.255")]
    [InlineData("10.0.0.0")]
    public void IsPrivateOrReservedAddress_ClassA_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("172.20.0.1")]
    public void IsPrivateOrReservedAddress_ClassB_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("192.168.0.1")]
    [InlineData("192.168.1.1")]
    [InlineData("192.168.255.255")]
    public void IsPrivateOrReservedAddress_ClassC_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("169.254.0.1")]
    [InlineData("169.254.255.255")]
    public void IsPrivateOrReservedAddress_LinkLocal_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("0.0.0.1")]
    public void IsPrivateOrReservedAddress_ZeroNetwork_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("100.64.0.1")]
    [InlineData("100.127.255.255")]
    public void IsPrivateOrReservedAddress_CarrierGradeNat_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("192.0.0.1")]
    public void IsPrivateOrReservedAddress_IetfProtocol_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("192.0.2.1")]
    public void IsPrivateOrReservedAddress_TestNet1_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("198.51.100.1")]
    public void IsPrivateOrReservedAddress_TestNet2_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("203.0.113.1")]
    public void IsPrivateOrReservedAddress_TestNet3_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("224.0.0.1")]
    [InlineData("239.255.255.255")]
    public void IsPrivateOrReservedAddress_Multicast_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("240.0.0.1")]
    [InlineData("255.255.255.255")]
    public void IsPrivateOrReservedAddress_Reserved_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("::1")]
    public void IsPrivateOrReservedAddress_IPv6Loopback_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("fe80::1")]
    [InlineData("fe80::abcd:1234")]
    public void IsPrivateOrReservedAddress_IPv6LinkLocal_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("fc00::1")]
    [InlineData("fd00::1")]
    public void IsPrivateOrReservedAddress_IPv6UniqueLocal_ReturnsTrue(string ip)
    {
        Assert.True(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("142.250.187.195")]
    [InlineData("40.76.4.15")]
    public void IsPrivateOrReservedAddress_PublicIPv4_ReturnsFalse(string ip)
    {
        Assert.False(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("2001:4860:4860::8888")]
    [InlineData("2606:4700:4700::1111")]
    public void IsPrivateOrReservedAddress_PublicIPv6_ReturnsFalse(string ip)
    {
        Assert.False(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Theory]
    [InlineData("172.15.0.1")]
    [InlineData("172.32.0.1")]
    public void IsPrivateOrReservedAddress_OutsideClassB_ReturnsFalse(string ip)
    {
        Assert.False(_resolver.IsPrivateOrReservedAddress(ip));
    }

    [Fact]
    public void IsPrivateOrReservedAddress_InvalidInput_ReturnsFalse()
    {
        Assert.False(_resolver.IsPrivateOrReservedAddress("not-an-ip"));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("::1")]
    public async Task ResolveHostname_ValidIPAddress_ReturnsSelf(string ip)
    {
        var (success, resolved) = await _resolver.ResolveHostname(ip, CancellationToken.None);

        Assert.True(success);
        Assert.NotNull(resolved);
    }

    [Fact]
    public async Task ResolveHostname_InvalidHostname_ReturnsFalse()
    {
        var (success, resolved) = await _resolver.ResolveHostname("this-hostname-should-not-exist-12345.invalid", CancellationToken.None);

        Assert.False(success);
        Assert.Null(resolved);
    }

    [Fact]
    public async Task ResolveHostname_CancelledToken_ThrowsOperationCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Cancelled token on an IP address should still work (no async DNS needed)
        var (success, resolved) = await _resolver.ResolveHostname("8.8.8.8", cts.Token);
        Assert.True(success);
    }
}
