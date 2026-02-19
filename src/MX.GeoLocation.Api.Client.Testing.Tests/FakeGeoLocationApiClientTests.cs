using MX.GeoLocation.Api.Client.Testing;
using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;

namespace MX.GeoLocation.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class FakeGeoLocationApiClientTests
{
    [Fact]
    public async Task GeoLookup_V1_DelegatesToV1Fake()
    {
        var client = new FakeGeoLocationApiClient();
        client.V1Lookup.AddResponse("8.8.8.8",
            GeoLocationDtoFactory.CreateGeoLocation(cityName: "Mountain View"));

        var result = await client.GeoLookup.V1.GetGeoLocation("8.8.8.8");

        Assert.Equal("Mountain View", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GeoLookup_V1_1_DelegatesToV1_1Fake()
    {
        var client = new FakeGeoLocationApiClient();
        client.V1_1Lookup.AddCityResponse("1.1.1.1",
            GeoLocationDtoFactory.CreateCityGeoLocation(cityName: "Los Angeles"));

        var result = await client.GeoLookup.V1_1.GetCityGeoLocation("1.1.1.1");

        Assert.Equal("Los Angeles", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task ApiInfo_DelegatesToInfoFake()
    {
        var client = new FakeGeoLocationApiClient();
        client.InfoApi.WithInfo(GeoLocationDtoFactory.CreateApiInfo(buildVersion: "2.0.0.1"));

        var result = await client.ApiInfo.GetApiInfo();

        Assert.Equal("2.0.0.1", result.Result!.Data!.BuildVersion);
    }

    [Fact]
    public async Task ApiHealth_DelegatesToHealthFake()
    {
        var client = new FakeGeoLocationApiClient();
        client.HealthApi.WithStatusCode(System.Net.HttpStatusCode.ServiceUnavailable);

        var result = await client.ApiHealth.CheckHealth();

        Assert.Equal(System.Net.HttpStatusCode.ServiceUnavailable, result.StatusCode);
    }

    [Fact]
    public void ImplementsIGeoLocationApiClient()
    {
        IGeoLocationApiClient client = new FakeGeoLocationApiClient();

        Assert.NotNull(client.GeoLookup);
        Assert.NotNull(client.GeoLookup.V1);
        Assert.NotNull(client.GeoLookup.V1_1);
        Assert.NotNull(client.ApiInfo);
        Assert.NotNull(client.ApiHealth);
    }

    [Fact]
    public async Task Reset_ClearsAllFakeState()
    {
        var client = new FakeGeoLocationApiClient();
        client.V1Lookup.AddResponse("8.8.8.8",
            GeoLocationDtoFactory.CreateGeoLocation(cityName: "Configured"));
        client.V1_1Lookup.AddCityResponse("8.8.8.8",
            GeoLocationDtoFactory.CreateCityGeoLocation(cityName: "Configured"));
        await client.GeoLookup.V1.GetGeoLocation("8.8.8.8");

        client.Reset();

        Assert.Empty(client.V1Lookup.LookedUpAddresses);
        var result = await client.GeoLookup.V1.GetGeoLocation("8.8.8.8");
        Assert.Equal("Test City", result.Result!.Data!.CityName);
        var cityResult = await client.GeoLookup.V1_1.GetCityGeoLocation("8.8.8.8");
        Assert.Equal("Test City", cityResult.Result!.Data!.CityName);
    }
}
