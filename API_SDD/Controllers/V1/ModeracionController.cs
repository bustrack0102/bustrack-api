using System.Diagnostics.CodeAnalysis;
using ApiSDD.Services.Moderation;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Evaluación y moderación Cloud (texto, imagen y video) previa al almacenamiento (Lineamiento 16).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/moderacion")]
public sealed class ModeracionController : ControllerBase
{
    private const long MaxInspectBytes = 26L * 1024 * 1024;
    private readonly IContentModerationService _moderation;

    public ModeracionController(IContentModerationService moderation)
    {
        _moderation = moderation ?? throw new ArgumentNullException(nameof(moderation));
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> EvaluarContenido(
        [FromBody] ModeracionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Texto) && string.IsNullOrWhiteSpace(request.ImagenUrl))
        {
            return BadRequest(new { error = "Se requiere al menos un campo de texto o URL de imagen para moderar.", status = 400 });
        }

        var decision = await _moderation.ModerateUrlAndTextAsync(
            request.Texto,
            request.ImagenUrl,
            request.TipoContenido,
            cancellationToken).ConfigureAwait(false);

        return Ok(new ModeracionResponse(
            decision.Aprobado,
            decision.Motivo,
            decision.PuntuacionSeguridad,
            decision.InfraccionesDetectadas,
            decision.Proveedor));
    }

    [HttpPost("archivo")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    [RequestSizeLimit(MaxInspectBytes)]
    public async Task<IActionResult> EvaluarArchivo(
        IFormFile? archivo,
        IFormFile? previewFrame = null,
        [FromForm] string? notas = null,
        CancellationToken cancellationToken = default)
    {
        if (archivo is null || archivo.Length <= 0)
        {
            return BadRequest(new { error = "Se requiere un archivo válido para moderación.", status = 400 });
        }

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        byte[]? frameBytes = null;
        if (previewFrame is not null && previewFrame.Length > 0)
        {
            using var fms = new MemoryStream();
            await previewFrame.CopyToAsync(fms, cancellationToken).ConfigureAwait(false);
            frameBytes = fms.ToArray();
        }

        var decision = await _moderation.ModerateUploadedMediaAsync(
            ms.ToArray(),
            archivo.FileName,
            archivo.ContentType,
            frameBytes,
            notas,
            cancellationToken).ConfigureAwait(false);

        var response = new ModeracionResponse(
            decision.Aprobado,
            decision.Motivo,
            decision.PuntuacionSeguridad,
            decision.InfraccionesDetectadas,
            decision.Proveedor);

        return decision.Aprobado ? Ok(response) : UnprocessableEntity(response);
    }
}

[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON público.")]
public sealed record ModeracionRequest(
    string? Texto,
    string? TipoContenido,
    string? ImagenUrl);

public sealed record ModeracionResponse(
    bool Aprobado,
    string Motivo,
    double PuntuacionSeguridad,
    IReadOnlyList<string> InfraccionesDetectadas,
    string Proveedor = "PolicyEngine");
