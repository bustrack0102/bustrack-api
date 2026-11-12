using ApiSDD.Services.Storage;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Carga segura de imágenes y videos cortos hacia Blob CDN tras moderación Cloud obligatoria (Reqs. 15 y 16).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/media")]
public sealed class MediaController : ControllerBase
{
    private const long MaxUploadRequestBytes = 26L * 1024 * 1024;
    private readonly IBlobMediaService _media;

    public MediaController(IBlobMediaService media)
    {
        _media = media ?? throw new ArgumentNullException(nameof(media));
    }

    [HttpPost("upload")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    [RequestSizeLimit(MaxUploadRequestBytes)]
    public async Task<IActionResult> Upload(
        IFormFile? archivo,
        [FromForm] string categoria = "resenas",
        IFormFile? previewFrame = null,
        [FromForm] string? notas = null,
        CancellationToken cancellationToken = default)
    {
        if (archivo is null || archivo.Length <= 0)
        {
            return BadRequest(new { error = "Se requiere un archivo de imagen o video.", status = 400 });
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

        var result = await _media.ModerateAndUploadAsync(
            ms.ToArray(),
            archivo.FileName,
            archivo.ContentType,
            categoria,
            frameBytes,
            notas,
            cancellationToken).ConfigureAwait(false);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Url))
        {
            return UnprocessableEntity(new
            {
                error = "content_moderation_rejected",
                motivo = result.Moderation.Motivo,
                infracciones = result.Moderation.InfraccionesDetectadas,
                status = 422,
            });
        }

        return Created(new Uri(result.Url, UriKind.Absolute), result);
    }
}
