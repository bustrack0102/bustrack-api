using ApiSDD.Dtos.Operational;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Viajes guardados sincronizables con SQLite móvil (EP-VG-01 a EP-VG-05).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/viajes")]
public sealed class ViajesController : ControllerBase
{
    private readonly IUserInteractionService _service;

    public ViajesController(IUserInteractionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] bool soloFavoritos = false,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListViajesAsync(User, soloFavoritos, q, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken cancellationToken)
    {
        var detail = await _service.GetViajeDetalleAsync(User, id, cancellationToken).ConfigureAwait(false);
        return Ok(detail);
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] ViajeCreateRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateViajeAsync(User, request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/viajes/{created.Id}", UriKind.Relative), created);
    }

    [HttpPut("{id}")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] ViajeUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateViajeAsync(User, id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        await _service.DeleteViajeAsync(User, id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
