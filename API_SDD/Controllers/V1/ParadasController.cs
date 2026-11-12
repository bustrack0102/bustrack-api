using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Controlador NoSQL de paradas con búsqueda espacial 2dsphere y autocompletado (EP-RP-01 a EP-RP-03).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/paradas")]
public sealed class ParadasController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public ParadasController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/paradas (EP-RP-02)
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] string? municipio = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListParadasAsync(municipio, cancellationToken).ConfigureAwait(false);
        Response.Headers.Append("Cache-Control", "public, max-age=120");
        return Ok(result);
    }

    // GET /api/v1/paradas/autocompletar (EP-RP-01)
    [HttpGet("autocompletar")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Autocompletar(
        [FromQuery] string q = "",
        [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.AutocompleteParadasAsync(q, limite, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/paradas/cercanas?lat=20.05&lon=-99.34&radioKm=5
    [HttpGet("cercanas")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetCercanas(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radioKm = 10.0,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.GetParadasCercanasAsync(lat, lon, radioKm, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/paradas/{id} (EP-RP-03)
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var item = await _catalog.GetParadaByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return NotFound(new { error = "Parada no encontrada.", status = 404 });
        }

        return Ok(item);
    }
}
