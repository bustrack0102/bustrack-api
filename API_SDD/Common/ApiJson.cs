using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ApiSDD.Serialization;

namespace ApiSDD.Common;

/// <summary>Opciones JSON compartidas: convención legacy (sin camelCase) + Detail nulo omitido + SourceGen.</summary>
internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        opts.TypeInfoResolverChain.Insert(0, BusTrackJsonContext.Default);
        opts.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
        return opts;
    }
}
