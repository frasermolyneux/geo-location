using MX.GeoLocation.LookupWebApi.Models;
using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Tests.Models;

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
}
