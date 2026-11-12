using ApiSDD.Dtos.Operational;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Administración de fuentes de datos e importaciones de catálogo (EP-AD-02 a EP-AD-07).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = "AdminPolicy")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminCatalogService _adminService;

    public AdminController(IAdminCatalogService adminService)
    {
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
    }

    [HttpGet("fuentes-datos")]
    [EnableRateLimiting("Admin")]
    public async Task<IActionResult> ListFuentes([FromQuery] bool? soloActivas = null, CancellationToken cancellationToken = default)
    {
        var items = await _adminService.ListFuentesAsync(soloActivas, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpPost("fuentes-datos")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> CreateFuente(
        [FromBody] FuenteDatoUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _adminService.CreateFuenteAsync(request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/admin/fuentes-datos/{created.Id}", UriKind.Relative), created);
    }

    [HttpPut("fuentes-datos/{id:guid}")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> UpdateFuente(
        [FromRoute] Guid id,
        [FromBody] FuenteDatoUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _adminService.UpdateFuenteAsync(id, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpDelete("fuentes-datos/{id:guid}")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> DeleteFuente([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeactivateFuenteAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("importaciones")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> ExecuteImportacion(
        [FromBody] ImportacionExecuteRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _adminService.ExecuteImportacionAsync(User, request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/admin/importaciones/{created.Id}", UriKind.Relative), created);
    }

    [HttpGet("importaciones")]
    [EnableRateLimiting("Admin")]
    public async Task<IActionResult> ListImportaciones(
        [FromQuery] Guid? fuenteDatoId = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _adminService.ListImportacionesAsync(fuenteDatoId, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }
}
