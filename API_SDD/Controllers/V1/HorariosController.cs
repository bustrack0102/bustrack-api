using ApiSDD.Dtos.Operational;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Consulta de horarios y próxima salida (EP-HT-01, EP-HT-02).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/horarios")]
public sealed class HorariosController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public HorariosController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/horarios (EP-HT-01)
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? rutaId = null,
        [FromQuery] string? diaSemana = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListHorariosAsync(rutaId, diaSemana, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/horarios/siguiente (EP-HT-02)
    [HttpGet("siguiente")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetSiguiente(
        [FromQuery] Guid rutaId,
        [FromQuery] string? horaActual = null,
        CancellationToken cancellationToken = default)
    {
        if (rutaId == Guid.Empty)
        {
            return BadRequest(new { error = "El parámetro rutaId es obligatorio.", status = 400 });
        }

        var result = await _catalog.GetProximaSalidaAsync(rutaId, horaActual, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(new { error = "No existen horarios registrados para la ruta.", status = 404 });
        }

        return Ok(result);
    }

    // POST /api/v1/horarios (EP-HT-03)
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] HorarioUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalog.CreateHorarioAsync(request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/horarios/{created.Id}", UriKind.Relative), created);
    }

    // PUT /api/v1/horarios/{id} (EP-HT-04)
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] HorarioUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalog.UpdateHorarioAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    // DELETE /api/v1/horarios/{id} (EP-HT-05)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        await _catalog.DeactivateHorarioAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
