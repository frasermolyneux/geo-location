using System.Text.RegularExpressions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MX.GeoLocation.LookupWebApi.OpenApi;

/// <summary>
/// Strips the version prefix (e.g. /v1/) from OpenAPI paths so that
/// APIM segment-based versioning can manage the version segment.
/// </summary>
public partial class StripVersionPrefixTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var originalPaths = document.Paths.ToList();
        document.Paths.Clear();

        foreach (var (path, pathItem) in originalPaths)
        {
            var newPath = VersionPrefixRegex().Replace(path, "/");
            document.Paths[newPath] = pathItem;
        }

        return Task.CompletedTask;
    }

    [GeneratedRegex(@"^/v\d+(\.\d+)?/")]
    private static partial Regex VersionPrefixRegex();
}
