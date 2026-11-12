using System.Text.Json.Serialization;
using ApiSDD.Common;

namespace ApiSDD.Serialization;

/// <summary>
/// Contexto de serialización JSON generado en tiempo de compilación (Req. 15 Json serializers).
/// Conserva PropertyNamingPolicy = Unspecified (legacy PascalCase) y omisión de nulos.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(PagedResult<object>))]
internal sealed partial class BusTrackJsonContext : JsonSerializerContext
{
}
