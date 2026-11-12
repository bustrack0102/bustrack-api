using ApiSDD.Dtos.Operational;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Controlador NoSQL de rutas de transporte público (EP-RP-04 a EP-RP-09 + EP-HT).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rutas")]
public sealed class RutasController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public RutasController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/rutas (EP-RP-06)
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetRutas(
        [FromQuery] string? empresa = null,
        [FromQuery] string? destino = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListRutasAsync(empresa, destino, pagina, tamanoPagina, cancellationToken)
            .ConfigureAwait(false);
        Response.Headers.Append("Cache-Control", "public, max-age=60");
        return Ok(result);
    }

    // GET /api/v1/rutas/buscar (EP-RP-04)
    [HttpGet("buscar")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? origen = null,
        [FromQuery] string? destino = null,
        [FromQuery] string? municipio = null,
        [FromQuery] string? medioTransporte = null,
        [FromQuery] decimal? tarifaMax = null,
        [FromQuery] string? ordenarPor = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.SearchRutasAsync(
            origen, destino, municipio, medioTransporte, tarifaMax, ordenarPor, cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/rutas/autocompletar (EP-RP-05)
    [HttpGet("autocompletar")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Autocompletar(
        [FromQuery] string q = "",
        [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.AutocompleteRutasAsync(q, limite, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/rutas/{id} (EP-RP-07)
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetRutaPorId([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ruta = await _catalog.GetRutaByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (ruta is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        return Ok(ruta);
    }

    // GET /api/v1/rutas/{id}/paradas (EP-RP-08)
    [HttpGet("{id:guid}/paradas")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetParadasPorRuta([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _catalog.GetRutaParadasAsync(id, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        return Ok(result);
    }

    // GET /api/v1/rutas/{id}/geometria (EP-RP-09)
    [HttpGet("{id:guid}/geometria")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetGeometria([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _catalog.GetRutaGeometriaAsync(id, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        return Ok(result);
    }

    // GET /api/v1/rutas/{id}/horarios (spec 21)
    [HttpGet("{id:guid}/horarios")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetHorariosRuta(
        [FromRoute] Guid id,
        [FromQuery] string? dia = null,
        CancellationToken cancellationToken = default)
    {
        var ruta = await _catalog.GetRutaByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (ruta is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        var items = await _catalog.ListHorariosAsync(id, dia, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    // GET /api/v1/rutas/{id}/tarifas (spec 22)
    [HttpGet("{id:guid}/tarifas")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetTarifasRuta(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var ruta = await _catalog.GetRutaByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (ruta is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        var items = await _catalog.ListTarifasAsync(id, true, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    // POST /api/v1/rutas (EP-RP-14)
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] RutaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalog.CreateRutaAsync(request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/rutas/{created.Id}", UriKind.Relative), created);
    }

    // PUT /api/v1/rutas/{id} (EP-RP-15)
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] RutaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalog.UpdateRutaAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    // DELETE /api/v1/rutas/{id} (EP-RP-16)
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _catalog.DeactivateRutaAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
