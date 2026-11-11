using System.Text.RegularExpressions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Evaluación y moderación de contenido previo al almacenamiento (Lineamiento 16).
/// Valida lenguaje inapropiado, prompt injections, PII y material no permitido.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/moderacion")]
public sealed partial class ModeracionController : ControllerBase
{
    private static readonly string[] PalabrasInapropiadas =
    [
        "groseria", "insulto", "estafa", "fraude", "odio", "violencia", "discriminacion", "arma", "droga"
    ];

    [GeneratedRegex(@"\b(?:ignora\s+(?:todas\s+)?(?:las\s+)?instrucciones|system\s*prompt|jailbreak)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PromptInjectionRegex();

    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:\+?52[-.\s]?)?(?:\d{2,3}[-.\s]?){2}\d{4}\b")]
    private static partial Regex PhoneRegex();

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult EvaluarContenido([FromBody] ModeracionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Texto) && string.IsNullOrWhiteSpace(request.ImagenUrl))
        {
            return BadRequest(new { error = "Se requiere al menos un campo de texto o URL de imagen para moderar.", status = 400 });
        }

        var texto = request.Texto ?? string.Empty;
        var infracciones = new List<string>();

        // 1. Detección de Prompt Injection (Req 6/16)
        if (PromptInjectionRegex().IsMatch(texto))
        {
            infracciones.Add("Intento de manipulación de instrucciones (Prompt Injection).");
        }

        // 2. Detección de lenguaje ofensivo o inapropiado (Req 16)
        foreach (var palabra in PalabrasInapropiadas.Where(p => texto.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            infracciones.Add($"Uso de término inapropiado: '{palabra}'.");
        }

        // 3. Detección de PII sensible no consentido en texto público
        if (EmailRegex().IsMatch(texto) || PhoneRegex().IsMatch(texto))
        {
            infracciones.Add("Presencia de datos personales sensibles (teléfono o correo electrónico).");
        }

        // 4. Verificación de imagen
        if (!string.IsNullOrWhiteSpace(request.ImagenUrl) &&
            (!Uri.TryCreate(request.ImagenUrl, UriKind.Absolute, out var uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            infracciones.Add("URL de imagen con protocolo no seguro o formato inválido.");
        }

        var aprobado = infracciones.Count == 0;
        var puntuacion = aprobado ? 0.98 : 0.20;
        var motivo = aprobado ? "Contenido validado como seguro y respetuoso." : "Contenido no permitido por políticas comunitarias.";

        return Ok(new ModeracionResponse(
            aprobado,
            motivo,
            puntuacion,
            infracciones
        ));
    }
}

public sealed record ModeracionRequest(
    string? Texto,
    string? TipoContenido, // "comentario", "reporte", "perfil"
    string? ImagenUrl
);

public sealed record ModeracionResponse(
    bool Aprobado,
    string Motivo,
    double PuntuacionSeguridad,
    IReadOnlyList<string> InfraccionesDetectadas
);
