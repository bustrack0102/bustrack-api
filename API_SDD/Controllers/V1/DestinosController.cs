using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Catálogo oficial de destinos turísticos e interés conectados a rutas (EP-DE-01 a EP-DE-05).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/destinos")]
public sealed class DestinosController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public DestinosController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/destinos/autocompletar (EP-DE-01)
    [HttpGet("autocompletar")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Autocompletar(
        [FromQuery] string q = "",
        [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.AutocompleteDestinosAsync(q, limite, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/destinos (EP-DE-02)
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] string? categoria = null,
        [FromQuery] string? municipio = null,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListDestinosAsync(categoria, municipio, q, cancellationToken).ConfigureAwait(false);
        Response.Headers.Append("Cache-Control", "public, max-age=120");
        return Ok(result);
    }

    // GET /api/v1/destinos/categorias (EP-DE-03)
    [HttpGet("categorias")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Categorias(CancellationToken cancellationToken = default)
    {
        var result = await _catalog.GetCategoriasDestinosAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/destinos/{id} (EP-DE-04)
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var item = await _catalog.GetDestinoByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return NotFound(new { error = "Destino no encontrado.", status = 404 });
        }

        return Ok(item);
    }

    // GET /api/v1/destinos/{id}/rutas (EP-DE-05)
    [HttpGet("{id}/rutas")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetRutas([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var result = await _catalog.GetRutasDeDestinoAsync(id, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(new { error = "Destino no encontrado.", status = 404 });
        }

        return Ok(result);
    }
}
