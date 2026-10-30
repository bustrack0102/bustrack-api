using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiSDD.Common;

/// <summary>Opciones JSON compartidas: convención legacy (sin camelCase) + Detail nulo omitido.</summary>
internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
