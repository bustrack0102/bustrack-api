using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Consulta y validación administrativa de paradas candidatas detectadas por ML (EP-ML-07, EP-AD-08).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/candidatos-parada")]
[Authorize(Policy = "AdminPolicy")]
public sealed class CandidatosParadaController : ControllerBase
{
    private readonly IIntelligenceService _intelligence;

    public CandidatosParadaController(IIntelligenceService intelligence)
    {
        _intelligence = intelligence ?? throw new ArgumentNullException(nameof(intelligence));
    }

    [HttpGet]
    [EnableRateLimiting("Admin")]
    public async Task<IActionResult> List(
        [FromQuery] string? estado = null,
        [FromQuery] Guid? rutaId = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] double radioKm = 5.0,
        CancellationToken cancellationToken = default)
    {
        var items = await _intelligence.ListCandidatosParadaAsync(
            estado,
            rutaId,
            lat,
            lon,
            radioKm,
            cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    // PATCH /api/v1/admin/candidatos-parada/{id}/validar (EP-AD-08)
    [HttpPatch("/api/v{version:apiVersion}/admin/candidatos-parada/{id}/validar")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Validar(
        [FromRoute] string id,
        [FromBody] ValidarCandidatoParadaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _intelligence.ValidateCandidatoParadaAsync(id, request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }
}
