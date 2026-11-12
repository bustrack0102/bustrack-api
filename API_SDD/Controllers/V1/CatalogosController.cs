using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Sincronización incremental de catálogos SQLite y sonda REST v1 (EP-AD-01, EP-AD-09).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/catalogos")]
public sealed class CatalogosController : ControllerBase
{
    private readonly ITransportCatalogService _catalog;

    public CatalogosController(ITransportCatalogService catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    // GET /api/v1/catalogos/estado-actualizacion (EP-AD-01)
    [HttpGet("estado-actualizacion")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetEstadoActualizacion(CancellationToken cancellationToken = default)
    {
        var estado = await _catalog.GetEstadoActualizacionAsync(cancellationToken).ConfigureAwait(false);
        Response.Headers.Append("Cache-Control", "public, max-age=30");
        return Ok(estado);
    }

    // GET /api/v1/health (EP-AD-09)
    [HttpGet("/api/v{version:apiVersion}/health")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetApiHealth(
        [FromServices] HealthCheckService healthChecks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(healthChecks);
        var report = await healthChecks.CheckHealthAsync(cancellationToken).ConfigureAwait(false);
        var payload = new
        {
            status = report.Status.ToString(),
            version = "1.0",
            timestampUtc = DateTime.UtcNow,
        };
        return report.Status == HealthStatus.Unhealthy
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, payload)
            : Ok(payload);
    }
}
