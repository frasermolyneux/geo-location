using Microsoft.OpenApi.Models;
using MX.GeoLocation.LookupWebApi.OpenApi;

namespace MX.GeoLocation.LookupWebApi.Tests.OpenApi;

public class StripVersionPrefixTransformerTests
{
    private readonly StripVersionPrefixTransformer _transformer = new();

    [Theory]
    [InlineData("/v1.0/lookup/{hostname}", "/lookup/{hostname}")]
    [InlineData("/v1.1/lookup/city/{hostname}", "/lookup/city/{hostname}")]
    [InlineData("/v2/lookup/{hostname}", "/lookup/{hostname}")]
    [InlineData("/v1/info", "/info")]
    [InlineData("/v10.5/something", "/something")]
    public async Task TransformAsync_StripsVersionPrefix(string inputPath, string expectedPath)
    {
        var document = CreateDocumentWithPaths(inputPath);

        // The transformer does not use the context parameter, so null is safe here
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        Assert.True(document.Paths.ContainsKey(expectedPath),
            $"Expected path '{expectedPath}' not found. Actual paths: {string.Join(", ", document.Paths.Keys)}");
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/lookup/{hostname}")]
    public async Task TransformAsync_PathWithoutVersionPrefix_Unchanged(string path)
    {
        var document = CreateDocumentWithPaths(path);

        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        Assert.True(document.Paths.ContainsKey(path));
    }

    [Fact]
    public async Task TransformAsync_StrippedPathAlwaysStartsWithSlash()
    {
        var document = CreateDocumentWithPaths("/v1");

        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        var resultPath = Assert.Single(document.Paths).Key;
        Assert.StartsWith("/", resultPath);
    }

    [Fact]
    public async Task TransformAsync_MultiplePaths_AllStripped()
    {
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/v1.0/lookup/{hostname}"] = new OpenApiPathItem(),
                ["/v1.0/lookup"] = new OpenApiPathItem(),
                ["/v1.0/info"] = new OpenApiPathItem(),
            }
        };

        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        Assert.Equal(3, document.Paths.Count);
        Assert.True(document.Paths.ContainsKey("/lookup/{hostname}"));
        Assert.True(document.Paths.ContainsKey("/lookup"));
        Assert.True(document.Paths.ContainsKey("/info"));
    }

    [Fact]
    public async Task TransformAsync_PreservesPathItems()
    {
        var pathItem = new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new OpenApiOperation { Summary = "Test operation" }
            }
        };
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths { ["/v1.0/lookup"] = pathItem }
        };

        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        var resultItem = document.Paths["/lookup"];
        Assert.Equal("Test operation", resultItem.Operations[OperationType.Get].Summary);
    }

    private static OpenApiDocument CreateDocumentWithPaths(params string[] paths)
    {
        var doc = new OpenApiDocument { Paths = new OpenApiPaths() };
        foreach (var path in paths)
            doc.Paths.Add(path, new OpenApiPathItem());
        return doc;
    }
}
