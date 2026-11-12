using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Gestión explícita del consentimiento de privacidad para telemetría de sensores (EP-ML-01 a EP-ML-03).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/consentimientos")]
[Authorize]
public sealed class ConsentimientosController : ControllerBase
{
    private readonly IIntelligenceService _intelligence;

    public ConsentimientosController(IIntelligenceService intelligence)
    {
        _intelligence = intelligence ?? throw new ArgumentNullException(nameof(intelligence));
    }

    [HttpGet("me")]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _intelligence.GetConsentimientoAsync(User, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Grant(
        [FromBody] OtorgarConsentimientoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _intelligence.GrantConsentimientoAsync(User, request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpDelete("me")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        var result = await _intelligence.RevokeConsentimientoAsync(User, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
