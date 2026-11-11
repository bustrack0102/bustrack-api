using ApiSDD.Dtos;
using ApiSDD.Dtos.MedioTransporte;
using ApiSDD.Services.MedioTransporte;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Controlador para el catálogo de Medios de Transporte (EP-EM-09 a EP-EM-12).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/medios-transporte")]
public sealed class MediosTransporteController : ControllerBase
{
    private readonly IMedioTransporteService _service;

    public MediosTransporteController(IMedioTransporteService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // EP-EM-09: Catálogo de Medios de Transporte
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> List(
        [FromQuery] bool? activo = null,
        [FromQuery] string? municipio = null,
        [FromQuery] bool? conRutasActivas = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(activo, municipio, conRutasActivas, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // EP-EM-10: Alta en catálogo (Admin)
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Create(
        [FromBody] MedioTransporteCreateDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = await _service.CreateAsync(dto, cancellationToken).ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // EP-EM-11: Edición de medio de transporte (Admin)
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] MedioTransporteUpdateDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = await _service.UpdateAsync(id, dto, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // EP-EM-12: Baja de medio de transporte (Admin)
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        return Ok(new MessageResponseDto("Medio de transporte desactivado correctamente."));
    }
}
