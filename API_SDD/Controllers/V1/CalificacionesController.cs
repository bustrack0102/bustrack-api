using ApiSDD.Dtos.Operational;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Calificaciones y reseñas moderadas antes de almacenar en MongoDB (EP-CA-01 a EP-CA-06).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calificaciones")]
public sealed class CalificacionesController : ControllerBase
{
    private readonly IUserInteractionService _service;

    public CalificacionesController(IUserInteractionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? rutaId = null,
        [FromQuery] Guid? empresaId = null,
        [FromQuery] string? destinoId = null,
        [FromQuery] int? puntuacionMin = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListCalificacionesAsync(
            rutaId,
            empresaId,
            destinoId,
            puntuacionMin,
            pagina,
            tamanoPagina,
            cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("resumen")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Resumen(
        [FromQuery] Guid? rutaId = null,
        [FromQuery] Guid? empresaId = null,
        [FromQuery] string? destinoId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetResumenCalificacionesAsync(rutaId, empresaId, destinoId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("mias")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Mias(CancellationToken cancellationToken)
    {
        var result = await _service.ListMisCalificacionesAsync(User, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] CalificacionCreateRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateCalificacionAsync(User, request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/calificaciones/{created.Id}", UriKind.Relative), created);
    }

    [HttpPut("{id}")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] CalificacionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateCalificacionAsync(User, id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        await _service.DeleteCalificacionAsync(User, id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
