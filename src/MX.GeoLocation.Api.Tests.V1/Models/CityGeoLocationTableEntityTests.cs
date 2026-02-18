using MX.GeoLocation.LookupWebApi.Models;

namespace MX.GeoLocation.LookupWebApi.Tests.Models;

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
}
