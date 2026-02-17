using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MX.GeoLocation.LookupWebApi.OpenApi;

/// <summary>
/// Strips the /v1 prefix from OpenAPI spec paths so that APIM segment versioning
/// can manage the version prefix. Without this, APIM produces /v1/v1/... paths.
/// The backend still routes with /v1/ because APIM forwards the version segment.
/// </summary>
public class StripVersionPrefixTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var updatedPaths = new OpenApiPaths();

        foreach (var (path, pathItem) in document.Paths)
        {
            var newPath = path.StartsWith("/v1", StringComparison.OrdinalIgnoreCase)
                ? path[3..] // Remove "/v1" prefix
                : path;

            // Ensure the path still starts with /
            if (!newPath.StartsWith('/'))
                newPath = "/" + newPath;

            updatedPaths.Add(newPath, pathItem);
        }

        document.Paths = updatedPaths;
        return Task.CompletedTask;
    }
}
