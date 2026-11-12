using ApiSDD.Dtos.Operational;
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
        [FromQuery] Guid? rutaId = null,
        [FromQuery] bool? esTerminal = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double radioMetros = 800,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListParadasAsync(
            municipio, rutaId, esTerminal, lat, lng, radioMetros, pagina, tamanoPagina, cancellationToken)
            .ConfigureAwait(false);
        Response.Headers.Append("Cache-Control", "public, max-age=120");
        return Ok(result.Items);
    }

    // GET /api/v1/paradas/autocompletar (EP-RP-01)
    [HttpGet("autocompletar")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Autocompletar(
        [FromQuery] string q = "",
        [FromQuery] string? municipio = null,
        [FromQuery] bool? soloOrigen = null,
        [FromQuery] Guid? rutaId = null,
        [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.AutocompleteParadasAsync(q, municipio, soloOrigen, rutaId, limite, cancellationToken)
            .ConfigureAwait(false);
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

    // POST /api/v1/paradas (EP-RP-11)
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] ParadaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalog.CreateParadaAsync(request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/paradas/{created.Id}", UriKind.Relative), created);
    }

    // PUT /api/v1/paradas/{id} (EP-RP-12)
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] ParadaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalog.UpdateParadaAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    // DELETE /api/v1/paradas/{id} (EP-RP-13)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        await _catalog.DeactivateParadaAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
