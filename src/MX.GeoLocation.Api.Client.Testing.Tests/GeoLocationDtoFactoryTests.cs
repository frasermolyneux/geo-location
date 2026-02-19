using MX.GeoLocation.Api.Client.Testing;

namespace MX.GeoLocation.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class GeoLocationDtoFactoryTests
{
    [Fact]
    public void CreateGeoLocation_DefaultValues_PopulatesAllProperties()
    {
        var dto = GeoLocationDtoFactory.CreateGeoLocation();

        Assert.Equal("8.8.8.8", dto.Address);
        Assert.Equal("8.8.8.8", dto.TranslatedAddress);
        Assert.Equal("NA", dto.ContinentCode);
        Assert.Equal("North America", dto.ContinentName);
        Assert.Equal("US", dto.CountryCode);
        Assert.Equal("United States", dto.CountryName);
        Assert.False(dto.IsEuropeanUnion);
        Assert.Equal("Mountain View", dto.CityName);
        Assert.Equal("94035", dto.PostalCode);
        Assert.Equal(37.386, dto.Latitude);
        Assert.Equal(-122.0838, dto.Longitude);
        Assert.Equal(1000, dto.AccuracyRadius);
        Assert.Equal("America/Los_Angeles", dto.Timezone);
        Assert.NotNull(dto.Traits);
    }

    [Fact]
    public void CreateGeoLocation_CustomValues_OverridesDefaults()
    {
        var dto = GeoLocationDtoFactory.CreateGeoLocation(
            address: "1.1.1.1",
            cityName: "Los Angeles",
            countryName: "United States",
            latitude: 34.0522,
            longitude: -118.2437);

        Assert.Equal("1.1.1.1", dto.Address);
        Assert.Equal("Los Angeles", dto.CityName);
        Assert.Equal(34.0522, dto.Latitude);
        Assert.Equal(-118.2437, dto.Longitude);
    }

    [Fact]
    public void CreateCityGeoLocation_PopulatesSubdivisionsAndNetworkTraits()
    {
        var subdivisions = new List<string> { "California" };
        var dto = GeoLocationDtoFactory.CreateCityGeoLocation(
            subdivisions: subdivisions);

        Assert.Equal("Mountain View", dto.CityName);
        Assert.Equal(subdivisions, dto.Subdivisions);
        Assert.NotNull(dto.NetworkTraits);
        Assert.Equal("Google LLC", dto.NetworkTraits.Organization);
    }

    [Fact]
    public void CreateInsightsGeoLocation_PopulatesAnonymizer()
    {
        var anonymizer = GeoLocationDtoFactory.CreateAnonymizer(isAnonymousVpn: true);
        var dto = GeoLocationDtoFactory.CreateInsightsGeoLocation(anonymizer: anonymizer);

        Assert.True(dto.Anonymizer.IsAnonymousVpn);
        Assert.NotNull(dto.NetworkTraits);
    }

    [Fact]
    public void CreateNetworkTraits_DefaultValues_PopulatesExpectedFields()
    {
        var traits = GeoLocationDtoFactory.CreateNetworkTraits();

        Assert.Equal(15169, traits.AutonomousSystemNumber);
        Assert.Equal("Google LLC", traits.AutonomousSystemOrganization);
        Assert.Equal("Google LLC", traits.Isp);
        Assert.Equal("google.com", traits.Domain);
    }

    [Fact]
    public void CreateAnonymizer_AllFlags_SetCorrectly()
    {
        var anon = GeoLocationDtoFactory.CreateAnonymizer(
            isAnonymous: true,
            isAnonymousVpn: true,
            isTorExitNode: true,
            isHostingProvider: false);

        Assert.True(anon.IsAnonymous);
        Assert.True(anon.IsAnonymousVpn);
        Assert.True(anon.IsTorExitNode);
        Assert.False(anon.IsHostingProvider);
    }

    [Fact]
    public void CreateApiInfo_PopulatesVersionFields()
    {
        var info = GeoLocationDtoFactory.CreateApiInfo(
            version: "2.0.0",
            buildVersion: "2.0.0.42");

        Assert.Equal("2.0.0", info.Version);
        Assert.Equal("2.0.0.42", info.BuildVersion);
    }
}
