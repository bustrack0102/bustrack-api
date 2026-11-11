using ApiSDD.Dtos;
using ApiSDD.Dtos.Empresa;
using ApiSDD.Services.Empresa;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Controlador para el dominio de Empresas (EP-EM-01 a EP-EM-08).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/empresas")]
public sealed class EmpresasController : ControllerBase
{
    private readonly IEmpresaSqlService _service;

    public EmpresasController(IEmpresaSqlService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // GET /api/v1/empresas (EP-EM-01)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] EmpresaQueryDto query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var result = await _service.SearchAsync(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/empresas/autocompletar (EP-EM-02)
    [HttpGet("autocompletar")]
    [AllowAnonymous]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string q,
        [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.AutocompleteAsync(q, limite, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/empresas/comparar (EP-EM-05) - Rutas fijas antes de comodín {id}
    [HttpGet("comparar")]
    [AllowAnonymous]
    public async Task<IActionResult> Comparar(
        [FromQuery] string? paradaOrigenId = null,
        [FromQuery] string? paradaDestinoId = null,
        [FromQuery] string? destinoTuristicoId = null,
        [FromQuery] string? empresasIds = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CompararAsync(paradaOrigenId, paradaDestinoId, destinoTuristicoId, empresasIds, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/empresas/{id} (EP-EM-03)
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Detail(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return NotFound();
        }

        var result = await _service.GetDetailAsync(guid, cancellationToken).ConfigureAwait(false);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // GET /api/v1/empresas/{id}/rutas (EP-EM-04)
    [HttpGet("{id}/rutas")]
    [AllowAnonymous]
    public async Task<IActionResult> Rutas(
        string id,
        [FromQuery] int? medioTransporteId = null,
        [FromQuery] string? paradaId = null,
        [FromQuery] string? diaOperacion = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return NotFound();
        }

        var result = await _service.GetRutasAsync(guid, medioTransporteId, paradaId, diaOperacion, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // POST /api/v1/empresas (EP-EM-06)
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] EmpresaCreateDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var created = await _service.CreateAsync(dto, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(Detail), new { id = created.id.ToString() }, created);
    }

    // PUT /api/v1/empresas/{id} (EP-EM-07)
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] EmpresaUpdateDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (!Guid.TryParse(id, out var guid))
        {
            return NotFound();
        }

        var updated = await _service.UpdateAsync(guid, dto, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    // DELETE /api/v1/empresas/{id} (EP-EM-08)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return NotFound();
        }

        await _service.DeleteAsync(guid, cancellationToken).ConfigureAwait(false);
        return Ok(new MessageResponseDto("Empresa desactivada correctamente."));
    }
}
