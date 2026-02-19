using MX.GeoLocation.Api.Client.Testing;

namespace MX.GeoLocation.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class FakeGeoLookupApiV1_1Tests
{
    [Fact]
    public async Task GetCityGeoLocation_ConfiguredAddress_ReturnsCannedResponse()
    {
        var fake = new FakeGeoLookupApiV1_1();
        var expected = GeoLocationDtoFactory.CreateCityGeoLocation(address: "8.8.8.8", cityName: "Mountain View");
        fake.AddCityResponse("8.8.8.8", expected);

        var result = await fake.GetCityGeoLocation("8.8.8.8");

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("Mountain View", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetCityGeoLocation_UnconfiguredAddress_ReturnsDefaultResponse()
    {
        var fake = new FakeGeoLookupApiV1_1();

        var result = await fake.GetCityGeoLocation("192.168.1.1");

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("192.168.1.1", result.Result!.Data!.Address);
        Assert.Equal("Test City", result.Result!.Data!.CityName);
        Assert.Equal("Test Country", result.Result!.Data!.CountryName);
    }

    [Fact]
    public async Task GetCityGeoLocation_CaseInsensitiveLookup()
    {
        var fake = new FakeGeoLookupApiV1_1();
        fake.AddCityResponse("DNS.Google.COM", GeoLocationDtoFactory.CreateCityGeoLocation(cityName: "Custom City"));

        var result = await fake.GetCityGeoLocation("dns.google.com");

        Assert.Equal("Custom City", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_ConfiguredAddress_ReturnsCannedResponse()
    {
        var fake = new FakeGeoLookupApiV1_1();
        var anonymizer = GeoLocationDtoFactory.CreateAnonymizer(isAnonymousVpn: true);
        var expected = GeoLocationDtoFactory.CreateInsightsGeoLocation(
            address: "1.1.1.1", cityName: "Los Angeles", anonymizer: anonymizer);
        fake.AddInsightsResponse("1.1.1.1", expected);

        var result = await fake.GetInsightsGeoLocation("1.1.1.1");

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("Los Angeles", result.Result!.Data!.CityName);
        Assert.True(result.Result!.Data!.Anonymizer.IsAnonymousVpn);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_UnconfiguredAddress_ReturnsDefaultResponse()
    {
        var fake = new FakeGeoLookupApiV1_1();

        var result = await fake.GetInsightsGeoLocation("10.0.0.1");

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("10.0.0.1", result.Result!.Data!.Address);
        Assert.Equal("Test City", result.Result!.Data!.CityName);
    }

    [Fact]
    public async Task GetInsightsGeoLocation_CaseInsensitiveLookup()
    {
        var fake = new FakeGeoLookupApiV1_1();
        fake.AddInsightsResponse("DNS.Google.COM",
            GeoLocationDtoFactory.CreateInsightsGeoLocation(cityName: "Custom Insights"));

        var result = await fake.GetInsightsGeoLocation("dns.google.com");

        Assert.Equal("Custom Insights", result.Result!.Data!.CityName);
    }

    [Fact]
    public void AddCityResponse_FluentChaining()
    {
        var fake = new FakeGeoLookupApiV1_1()
            .AddCityResponse("8.8.8.8", GeoLocationDtoFactory.CreateCityGeoLocation())
            .AddCityResponse("1.1.1.1", GeoLocationDtoFactory.CreateCityGeoLocation(address: "1.1.1.1"));

        Assert.NotNull(fake);
    }

    [Fact]
    public void AddInsightsResponse_FluentChaining()
    {
        var fake = new FakeGeoLookupApiV1_1()
            .AddInsightsResponse("8.8.8.8", GeoLocationDtoFactory.CreateInsightsGeoLocation())
            .AddInsightsResponse("1.1.1.1", GeoLocationDtoFactory.CreateInsightsGeoLocation(address: "1.1.1.1"));

        Assert.NotNull(fake);
    }

    [Fact]
    public async Task CityAndInsights_IndependentResponses()
    {
        var fake = new FakeGeoLookupApiV1_1();
        fake.AddCityResponse("8.8.8.8", GeoLocationDtoFactory.CreateCityGeoLocation(cityName: "City Result"));
        fake.AddInsightsResponse("8.8.8.8", GeoLocationDtoFactory.CreateInsightsGeoLocation(cityName: "Insights Result"));

        var cityResult = await fake.GetCityGeoLocation("8.8.8.8");
        var insightsResult = await fake.GetInsightsGeoLocation("8.8.8.8");

        Assert.Equal("City Result", cityResult.Result!.Data!.CityName);
        Assert.Equal("Insights Result", insightsResult.Result!.Data!.CityName);
    }
}
