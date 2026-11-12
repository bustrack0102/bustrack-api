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
}
