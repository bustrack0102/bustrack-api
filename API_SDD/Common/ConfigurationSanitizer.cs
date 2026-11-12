using System.Globalization;

namespace ApiSDD.Common;

/// <summary>
/// Sanitiza valores de configuración sensibles (connection strings, endpoints, API keys)
/// eliminando caracteres de control, saltos de línea e invisibles Unicode heredados de secretos.
/// </summary>
internal static class ConfigurationSanitizer
{
    /// <summary>
    /// Limpia caracteres invisibles o saltos de línea (\r, \n, BOM, ancho cero) sin alterar el contenido útil.
    /// </summary>
    internal static string CleanSecret(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        Span<char> buffer = raw.Length <= 512 ? stackalloc char[raw.Length] : new char[raw.Length];
        var idx = 0;
        foreach (var ch in raw)
        {
            var cat = char.GetUnicodeCategory(ch);
            if (cat is UnicodeCategory.Control
                    or UnicodeCategory.Format
                    or UnicodeCategory.LineSeparator
                    or UnicodeCategory.ParagraphSeparator
                    or UnicodeCategory.Surrogate
                    or UnicodeCategory.PrivateUse
                    or UnicodeCategory.OtherNotAssigned
                || (cat == UnicodeCategory.SpaceSeparator && ch != ' '))
            {
                continue;
            }

            buffer[idx++] = ch;
        }

        return new string(buffer[..idx]).Trim();
    }

    /// <summary>
    /// Determina si una cadena ya sanitizada representa un secreto o URL real (no un placeholder).
    /// </summary>
    internal static bool HasUsableValue(string? cleaned, string? secondaryPlaceholder = null) =>
        !string.IsNullOrWhiteSpace(cleaned)
        && !cleaned.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
        && (secondaryPlaceholder is null
            || !cleaned.Contains(secondaryPlaceholder, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Adjunta la cabecera <c>X-Internal-Key</c> sanitizada para comunicación servicio-a-servicio segura.
    /// </summary>
    internal static void AttachInternalKeyHeader(HttpRequestMessage message, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(configuration);

        var internalKey = CleanSecret(configuration["InternalService:Key"]);
        if (HasUsableValue(internalKey))
        {
            message.Headers.Add("X-Internal-Key", internalKey);
        }
    }
}
