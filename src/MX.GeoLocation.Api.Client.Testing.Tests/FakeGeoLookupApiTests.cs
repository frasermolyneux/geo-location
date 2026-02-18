using MX.GeoLocation.Api.Client.Testing;

namespace MX.GeoLocation.Api.Client.Testing.Tests;

public class FakeGeoLookupApiTests
{
    [Fact]
    public async Task GetGeoLocation_ConfiguredAddress_ReturnsCannedResponse()
    {
        var fake = new FakeGeoLookupApi();
        var expected = GeoLocationDtoFactory.CreateGeoLocation(address: "8.8.8.8", cityName: "Mountain View");
        fake.AddResponse("8.8.8.8", expected);

        var result = await fake.GetGeoLocation("8.8.8.8");

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("Mountain View", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetGeoLocation_UnconfiguredAddress_ReturnsDefaultResponse()
    {
        var fake = new FakeGeoLookupApi();

        var result = await fake.GetGeoLocation("192.168.1.1");

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("192.168.1.1", result.Result!.Data!.Address);
        Assert.Equal("Test City", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetGeoLocation_CaseInsensitiveLookup()
    {
        var fake = new FakeGeoLookupApi();
        fake.AddResponse("DNS.Google.COM", GeoLocationDtoFactory.CreateGeoLocation(cityName: "Custom"));

        var result = await fake.GetGeoLocation("dns.google.com");

        Assert.Equal("Custom", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetGeoLocations_MixedConfiguredAndDefault_ReturnsBoth()
    {
        var fake = new FakeGeoLookupApi();
        fake.AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation(address: "8.8.8.8", cityName: "Mountain View"));

        var result = await fake.GetGeoLocations(["8.8.8.8", "1.1.1.1"]);

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        var items = result.Result!.Data!.Items!.ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal("Mountain View", items[0].CityName);
        Assert.Equal("Test City", items[1].CityName);
    }

    [Fact]
    public async Task DeleteMetadata_TracksDeletedAddresses()
    {
        var fake = new FakeGeoLookupApi();

        await fake.DeleteMetadata("8.8.8.8");
        await fake.DeleteMetadata("1.1.1.1");

        Assert.Contains("8.8.8.8", fake.DeletedAddresses);
        Assert.Contains("1.1.1.1", fake.DeletedAddresses);
        Assert.Equal(2, fake.DeletedAddresses.Count);
    }

    [Fact]
    public void AddResponse_FluentChaining()
    {
        var fake = new FakeGeoLookupApi()
            .AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation())
            .AddResponse("1.1.1.1", GeoLocationDtoFactory.CreateGeoLocation(address: "1.1.1.1"));

        Assert.NotNull(fake);
    }
}
