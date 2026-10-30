using ApiSDD.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiSDD.Health;

/// <summary>Respuesta /ready mínima en wire legacy: estado + resultados, sin trazas.</summary>
internal static class HealthResponses
{
    public static Task WriteReadyAsync(HttpContext context, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(report);
        context.Response.ContentType = "application/json";
        var payload = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["status"] = report.Status.ToString(),
            ["results"] = report.Entries.ToDictionary(
                static e => e.Key,
                static e => e.Value.Status.ToString(),
                StringComparer.Ordinal),
        };
        return context.Response.WriteAsJsonAsync(payload, ApiJson.Options, context.RequestAborted);
    }
}
