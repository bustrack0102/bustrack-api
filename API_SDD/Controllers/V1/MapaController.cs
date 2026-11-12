using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Elementos georreferenciados para visualización en mapa interactivo (EP-RP-10).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mapa")]
public sealed class MapaController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public MapaController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/mapa/elementos (EP-RP-10)
    [HttpGet("elementos")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetElementos(
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] double radioKm = 25.0,
        [FromQuery] string? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.GetElementosMapaAsync(lat, lon, radioKm, tipo, cancellationToken)
            .ConfigureAwait(false);
        Response.Headers.Append("Cache-Control", "public, max-age=120");
        return Ok(result);
    }
}
