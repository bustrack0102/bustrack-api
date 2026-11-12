using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Ingesta anónima de lotes de sensores inerciales y consulta administrativa ML (EP-ML-04, EP-ML-06).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sensores")]
public sealed class SensoresController : ControllerBase
{
    private readonly IIntelligenceService _intelligence;

    public SensoresController(IIntelligenceService intelligence)
    {
        _intelligence = intelligence ?? throw new ArgumentNullException(nameof(intelligence));
    }

    [HttpPost("lotes")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> SubmitBatch(
        [FromBody] LoteSensorCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _intelligence.SubmitSensorBatchAsync(User, request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/sensores/lotes/{result.LoteId}", UriKind.Relative), result);
    }

    [HttpGet("lotes")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("Admin")]
    public async Task<IActionResult> ListBatches(
        [FromQuery] Guid? rutaId = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _intelligence.ListLotesAsync(rutaId, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }
}
