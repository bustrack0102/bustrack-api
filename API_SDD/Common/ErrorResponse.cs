using System.Text.Json.Serialization;

namespace ApiSDD.Common;

/// <summary>Error uniforme en wire legacy: Detail solo no-prod (omitido cuando es nulo).</summary>
internal sealed record ErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("traceId")] string TraceId,
    [property: JsonPropertyName("detail"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Detail);
