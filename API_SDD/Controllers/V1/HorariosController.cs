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
}
