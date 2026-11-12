using ApiSDD.Dtos.Operational;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Consulta y cálculo de tarifas aproximadas en MXN (EP-HT-06, EP-HT-07).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tarifas")]
public sealed class TarifasController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public TarifasController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/tarifas (EP-HT-07)
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] Guid? rutaId = null,
        [FromQuery] bool? soloVigentes = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListTarifasAsync(rutaId, soloVigentes, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/tarifas/calcular (EP-HT-06)
    [HttpGet("calcular")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Calcular(
        [FromQuery] Guid rutaId,
        [FromQuery] string? tipoPasajero = "General",
        CancellationToken cancellationToken = default)
    {
        if (rutaId == Guid.Empty)
        {
            return BadRequest(new { error = "El parámetro rutaId es obligatorio.", status = 400 });
        }

        var result = await _catalog.CalcularTarifaAsync(rutaId, tipoPasajero, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(new { error = "Ruta o tarifa no encontrada.", status = 404 });
        }

        return Ok(result);
    }

    // POST /api/v1/tarifas (EP-HT-08)
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] TarifaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalog.CreateTarifaAsync(request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/tarifas/{created.Id}", UriKind.Relative), created);
    }

    // PUT /api/v1/tarifas/{id} (EP-HT-09)
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] TarifaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalog.UpdateTarifaAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    // DELETE /api/v1/tarifas/{id} (EP-HT-10)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        await _catalog.DeactivateTarifaAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
