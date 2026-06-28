using MX.GeoLocation.LookupWebApi.Services;

namespace MX.GeoLocation.Api.Tests.V1.Services;

[Trait("Category", "Unit")]
public class AddressNormalizerTests
{
    [Theory]
    [InlineData("[2001:4860:4860::8888]", "2001:4860:4860::8888")]
    [InlineData("fe80::1%eth0", "fe80::1")]
    [InlineData("[fe80::1%eth0]", "fe80::1")]
    [InlineData("8.8.8.8", "8.8.8.8")]
    public void NormalizeIpLiteral_ValidInputs_ReturnsCanonicalAddress(string input, string expected)
    {
        var result = AddressNormalizer.NormalizeIpLiteral(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("not-an-ip")]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeIpLiteral_NonIpInputs_ReturnsNull(string input)
    {
        var result = AddressNormalizer.NormalizeIpLiteral(input);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("[2606:4700:4700::1111]")]
    [InlineData("2606:4700:4700::1111")]
    [InlineData("1.1.1.1")]
    public void IsIpLiteral_ValidInputs_ReturnsTrue(string input)
    {
        Assert.True(AddressNormalizer.IsIpLiteral(input));
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("a.b.c.d")]
    [InlineData("[]")]
    public void IsIpLiteral_InvalidInputs_ReturnsFalse(string input)
    {
        Assert.False(AddressNormalizer.IsIpLiteral(input));
    }
}
