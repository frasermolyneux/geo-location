using MX.GeoLocation.LookupWebApi.Models;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.Api.Tests.V1.Models;

[Trait("Category", "Unit")]
public class CityGeoLocationTableEntityTests
{
    [Fact]
    public void ToCityDto_MalformedSubdivisions_ReturnsEmptyList()
    {
        var entity = new CityGeoLocationTableEntity
        {
            SubdivisionsSerialised = "broken-json!!!",
            NetworkTraitsSerialised = null,
            AnonymizerSerialised = null
        };

        var dto = entity.ToCityDto();

        Assert.Empty(dto.Subdivisions);
    }

    [Fact]
    public void ToCityDto_MalformedNetworkTraits_ReturnsDefault()
    {
        var entity = new CityGeoLocationTableEntity
        {
            SubdivisionsSerialised = null,
            NetworkTraitsSerialised = "{{invalid}}",
            AnonymizerSerialised = null
        };

        var dto = entity.ToCityDto();

        Assert.NotNull(dto.NetworkTraits);
    }

    [Fact]
    public void ToInsightsDto_MalformedAnonymizer_ReturnsDefault()
    {
        var entity = new CityGeoLocationTableEntity
        {
            SubdivisionsSerialised = null,
            NetworkTraitsSerialised = null,
            AnonymizerSerialised = "not-json"
        };

        var dto = entity.ToInsightsDto();

        Assert.NotNull(dto.Anonymizer);
    }

    [Fact]
    public void ToCityDto_NullSerialisedFields_ReturnsDefaults()
    {
        var entity = new CityGeoLocationTableEntity
        {
            SubdivisionsSerialised = null,
            NetworkTraitsSerialised = null,
            AnonymizerSerialised = null
        };

        var dto = entity.ToCityDto();

        Assert.Empty(dto.Subdivisions);
        Assert.NotNull(dto.NetworkTraits);
    }

    [Fact]
    public void Constructor_FromCityDto_SetsPartitionAndRowKey()
    {
        var dto = CreateFullCityDto();

        var entity = new CityGeoLocationTableEntity(dto);

        Assert.Equal("addresses", entity.PartitionKey);
        Assert.Equal(dto.TranslatedAddress, entity.RowKey);
    }

    [Fact]
    public void Roundtrip_CityDtoToEntityToDto_PreservesAllProperties()
    {
        var original = CreateFullCityDto();

        var entity = new CityGeoLocationTableEntity(original);
        var result = entity.ToCityDto();

        Assert.Equal(original.Address, result.Address);
        Assert.Equal(original.TranslatedAddress, result.TranslatedAddress);
        Assert.Equal(original.ContinentCode, result.ContinentCode);
        Assert.Equal(original.ContinentName, result.ContinentName);
        Assert.Equal(original.CountryCode, result.CountryCode);
        Assert.Equal(original.CountryName, result.CountryName);
        Assert.Equal(original.IsEuropeanUnion, result.IsEuropeanUnion);
        Assert.Equal(original.CityName, result.CityName);
        Assert.Equal(original.PostalCode, result.PostalCode);
        Assert.Equal(original.RegisteredCountry, result.RegisteredCountry);
        Assert.Equal(original.RepresentedCountry, result.RepresentedCountry);
        Assert.Equal(original.Latitude, result.Latitude);
        Assert.Equal(original.Longitude, result.Longitude);
        Assert.Equal(original.AccuracyRadius, result.AccuracyRadius);
        Assert.Equal(original.Timezone, result.Timezone);
        Assert.Equal(original.Subdivisions, result.Subdivisions);
        Assert.Equal(original.NetworkTraits.Isp, result.NetworkTraits.Isp);
        Assert.Equal(original.NetworkTraits.Organization, result.NetworkTraits.Organization);
    }

    [Fact]
    public void Roundtrip_InsightsDtoToEntityToDto_PreservesAnonymizer()
    {
        var original = new InsightsGeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            CityName = "Mountain View",
            CountryName = "United States",
            Subdivisions = ["California"],
            NetworkTraits = new NetworkTraitsDto { Isp = "Google LLC" },
            Anonymizer = new AnonymizerDto
            {
                IsAnonymous = true,
                IsAnonymousVpn = true,
                IsTorExitNode = false,
                IsHostingProvider = false,
                IsPublicProxy = false,
                IsResidentialProxy = false,
                ProviderName = "TestVPN"
            }
        };

        var entity = new CityGeoLocationTableEntity(original);
        var result = entity.ToInsightsDto();

        Assert.True(result.Anonymizer.IsAnonymous);
        Assert.True(result.Anonymizer.IsAnonymousVpn);
        Assert.False(result.Anonymizer.IsTorExitNode);
        Assert.Equal("TestVPN", result.Anonymizer.ProviderName);
    }

    [Fact]
    public void HasAnonymizerData_WhenInsightsDto_ReturnsTrue()
    {
        var dto = new InsightsGeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            Anonymizer = new AnonymizerDto()
        };

        var entity = new CityGeoLocationTableEntity(dto);

        Assert.True(entity.HasAnonymizerData);
    }

    [Fact]
    public void HasAnonymizerData_WhenCityDto_ReturnsFalse()
    {
        var dto = CreateFullCityDto();

        var entity = new CityGeoLocationTableEntity(dto);

        Assert.False(entity.HasAnonymizerData);
    }

    [Fact]
    public void Constructor_FromDto_ThrowsWhenTranslatedAddressIsNull()
    {
        var dto = new CityGeoLocationDto { Address = "8.8.8.8", TranslatedAddress = null };

        Assert.Throws<ArgumentNullException>(() => new CityGeoLocationTableEntity(dto));
    }

    private static CityGeoLocationDto CreateFullCityDto() => new()
    {
        Address = "google.co.uk",
        TranslatedAddress = "142.250.187.195",
        ContinentCode = "NA",
        ContinentName = "North America",
        CountryCode = "US",
        CountryName = "United States",
        IsEuropeanUnion = false,
        CityName = "Mountain View",
        PostalCode = "94035",
        RegisteredCountry = "US",
        RepresentedCountry = "",
        Latitude = 37.386,
        Longitude = -122.0838,
        AccuracyRadius = 1000,
        Timezone = "America/Los_Angeles",
        Subdivisions = ["California", "Santa Clara County"],
        NetworkTraits = new NetworkTraitsDto
        {
            Isp = "Google LLC",
            Organization = "Google LLC",
            AutonomousSystemNumber = 15169,
            Domain = "google.com"
        }
    };
}
