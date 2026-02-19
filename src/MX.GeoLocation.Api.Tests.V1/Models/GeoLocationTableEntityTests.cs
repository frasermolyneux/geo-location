using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.LookupWebApi.Models;
using Newtonsoft.Json;

namespace MX.GeoLocation.Api.Tests.V1.Models;

[Trait("Category", "Unit")]
public class GeoLocationTableEntityTests
{
    [Fact]
    public void Traits_ValidJson_ReturnsDeserializedDictionary()
    {
        var entity = new GeoLocationTableEntity
        {
            TraitsSerialised = JsonConvert.SerializeObject(new Dictionary<string, string?> { { "key", "value" } })
        };

        var traits = entity.Traits;

        Assert.Single(traits);
        Assert.Equal("value", traits["key"]);
    }

    [Fact]
    public void Traits_NullJson_ReturnsEmptyDictionary()
    {
        var entity = new GeoLocationTableEntity { TraitsSerialised = null };

        Assert.Empty(entity.Traits);
    }

    [Fact]
    public void Traits_MalformedJson_ReturnsEmptyDictionary()
    {
        var entity = new GeoLocationTableEntity { TraitsSerialised = "not-valid-json{{{" };

        Assert.Empty(entity.Traits);
    }

    [Fact]
    public void Traits_CachedOnSubsequentAccess()
    {
        var entity = new GeoLocationTableEntity
        {
            TraitsSerialised = JsonConvert.SerializeObject(new Dictionary<string, string?> { { "a", "b" } })
        };

        var first = entity.Traits;
        var second = entity.Traits;

        Assert.Same(first, second);
    }

    [Fact]
    public void Constructor_FromDto_SetsPartitionAndRowKey()
    {
        var dto = CreateFullDto();

        var entity = new GeoLocationTableEntity(dto);

        Assert.Equal("addresses", entity.PartitionKey);
        Assert.Equal(dto.TranslatedAddress, entity.RowKey);
    }

    [Fact]
    public void Roundtrip_DtoToEntityToDto_PreservesAllProperties()
    {
        var original = CreateFullDto();

        var entity = new GeoLocationTableEntity(original);
        var result = entity.GeoLocationDto();

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
        Assert.Equal(original.Latitude, result.Latitude);
        Assert.Equal(original.Longitude, result.Longitude);
        Assert.Equal(original.AccuracyRadius, result.AccuracyRadius);
        Assert.Equal(original.Timezone, result.Timezone);
        Assert.Equal(original.Traits, result.Traits);
    }

    [Fact]
    public void Roundtrip_DtoToEntityToDto_PreservesTraits()
    {
        var traits = new Dictionary<string, string?>
        {
            { "Isp", "Google LLC" },
            { "ConnectionType", null },
            { "AutonomousSystemNumber", "15169" }
        };
        var dto = new GeoLocationDto
        {
            Address = "8.8.8.8",
            TranslatedAddress = "8.8.8.8",
            Traits = traits
        };

        var entity = new GeoLocationTableEntity(dto);
        var result = entity.GeoLocationDto();

        Assert.Equal(3, result.Traits.Count);
        Assert.Equal("Google LLC", result.Traits["Isp"]);
        Assert.Null(result.Traits["ConnectionType"]);
        Assert.Equal("15169", result.Traits["AutonomousSystemNumber"]);
    }

    [Fact]
    public void Constructor_FromDto_ThrowsWhenTranslatedAddressIsNull()
    {
        var dto = new GeoLocationDto { Address = "8.8.8.8", TranslatedAddress = null };

        Assert.Throws<ArgumentNullException>(() => new GeoLocationTableEntity(dto));
    }

    private static GeoLocationDto CreateFullDto() => new()
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
        Latitude = 37.386,
        Longitude = -122.0838,
        AccuracyRadius = 1000,
        Timezone = "America/Los_Angeles",
        Traits = new Dictionary<string, string?> { { "Isp", "Google" }, { "UserType", null } }
    };
}
