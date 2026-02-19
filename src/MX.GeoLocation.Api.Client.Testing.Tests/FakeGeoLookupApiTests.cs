using MX.GeoLocation.Api.Client.Testing;

namespace MX.GeoLocation.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
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
    public async Task GetGeoLocations_WithErrorResponses_ReturnsPartialResultsWithErrors()
    {
        var fake = new FakeGeoLookupApi();
        fake.AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation(address: "8.8.8.8", cityName: "Mountain View"));
        fake.AddErrorResponse("invalid", System.Net.HttpStatusCode.BadRequest, "INVALID", "Invalid address");

        var result = await fake.GetGeoLocations(["8.8.8.8", "invalid"]);

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        var items = result.Result!.Data!.Items!.ToList();
        Assert.Single(items);
        Assert.Equal("Mountain View", items[0].CityName);
        Assert.NotNull(result.Result!.Errors);
        Assert.Single(result.Result!.Errors!);
        Assert.Equal("INVALID", result.Result!.Errors!.First().Code);
    }

    [Fact]
    public async Task GetGeoLocations_AllErrors_ReturnsEmptyItemsWithErrors()
    {
        var fake = new FakeGeoLookupApi();
        fake.AddErrorResponse("bad1", System.Net.HttpStatusCode.NotFound, "NOT_FOUND", "Not found");
        fake.AddErrorResponse("bad2", System.Net.HttpStatusCode.BadRequest, "BAD", "Bad request");

        var result = await fake.GetGeoLocations(["bad1", "bad2"]);

        Assert.Empty(result.Result!.Data!.Items!);
        Assert.Equal(2, result.Result!.Errors!.Count());
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

    [Fact]
    public async Task Reset_ClearsAllState()
    {
        var fake = new FakeGeoLookupApi();
        fake.AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation(cityName: "Configured"));
        fake.AddErrorResponse("bad", System.Net.HttpStatusCode.BadRequest, "ERR", "Error");
        await fake.GetGeoLocation("8.8.8.8");
        await fake.DeleteMetadata("8.8.8.8");

        fake.Reset();

        Assert.Empty(fake.LookedUpAddresses);
        Assert.Empty(fake.DeletedAddresses);
        // Previously configured address should now return default
        var result = await fake.GetGeoLocation("8.8.8.8");
        Assert.Equal("Test City", result.Result!.Data!.CityName);
        // Previously configured error should now return default success
        var errorResult = await fake.GetGeoLocation("bad");
        Assert.Equal(System.Net.HttpStatusCode.OK, errorResult.StatusCode);
    }

    [Fact]
    public async Task SetDefaultBehavior_ReturnError_UnconfiguredAddressReturnsError()
    {
        var fake = new FakeGeoLookupApi();
        fake.SetDefaultBehavior(DefaultLookupBehavior.ReturnError);
        fake.AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation(cityName: "Allowed"));

        var configuredResult = await fake.GetGeoLocation("8.8.8.8");
        Assert.Equal(System.Net.HttpStatusCode.OK, configuredResult.StatusCode);

        var unconfiguredResult = await fake.GetGeoLocation("unknown");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, unconfiguredResult.StatusCode);
    }

    [Fact]
    public async Task SetDefaultBehavior_ReturnError_CustomStatusCode()
    {
        var fake = new FakeGeoLookupApi();
        fake.SetDefaultBehavior(DefaultLookupBehavior.ReturnError,
            errorStatusCode: System.Net.HttpStatusCode.Forbidden,
            errorCode: "FORBIDDEN",
            errorMessage: "Not allowed");

        var result = await fake.GetGeoLocation("any-address");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task SetDefaultBehavior_ReturnError_BatchExcludesUnconfigured()
    {
        var fake = new FakeGeoLookupApi();
        fake.SetDefaultBehavior(DefaultLookupBehavior.ReturnError);
        fake.AddResponse("8.8.8.8", GeoLocationDtoFactory.CreateGeoLocation(address: "8.8.8.8", cityName: "Google"));

        var result = await fake.GetGeoLocations(["8.8.8.8", "unknown"]);

        var items = result.Result!.Data!.Items!.ToList();
        Assert.Single(items);
        Assert.Equal("Google", items[0].CityName);
        Assert.NotNull(result.Result!.Errors);
        Assert.Single(result.Result!.Errors!);
    }

    [Fact]
    public async Task Reset_AlsoResetsDefaultBehavior()
    {
        var fake = new FakeGeoLookupApi();
        fake.SetDefaultBehavior(DefaultLookupBehavior.ReturnError);

        fake.Reset();

        var result = await fake.GetGeoLocation("any");
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("Test City", result.Result!.Data!.CityName);
    }
}
