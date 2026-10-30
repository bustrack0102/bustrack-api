using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ApiSDD.OpenApi;

/// <summary>
/// Single-version document: runtime routes carry the {version} template,
/// but the published OpenAPI document exposes concrete v1 paths so API
/// clients never copy an unusable template.
/// </summary>
internal sealed class ApiVersionDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        foreach (var path in document.Paths.Keys.ToList())
        {
            if (!path.Contains("{version}", StringComparison.Ordinal))
            {
                continue;
            }

            var item = document.Paths[path]
                ?? throw new InvalidOperationException("OpenAPI path vanished during transform.");
            document.Paths.Remove(path);
            if (item.Operations is { } operations)
            {
                foreach (var operation in operations.Values)
                {
                    if (operation.Parameters is { } parameters)
                    {
                        var stale = parameters.FirstOrDefault(static p => p.Name == "version");
                        if (stale is not null)
                        {
                            parameters.Remove(stale);
                        }
                    }
                }
            }

            document.Paths[path.Replace("v{version}", "v1", StringComparison.Ordinal)
                .Replace("{version}", "1", StringComparison.Ordinal)] = item;
        }

        return Task.CompletedTask;
    }
}
