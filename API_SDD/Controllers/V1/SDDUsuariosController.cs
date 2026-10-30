using ApiSDD.Common;
using ApiSDD.Dtos;
using ApiSDD.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

// Controller name follows the versioned resource path; Sonar S101 cannot apply.
#pragma warning disable S101

namespace ApiSDD.Controllers.V1;

/// <summary>nv-04: user slice behind AdminPolicy. Errors flow to the middleware.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminPolicy")]
[EnableRateLimiting("Admin")]
public sealed class SDDUsuariosController : ControllerBase
{
    private readonly ISDDUsuarioService _service;

    public SDDUsuariosController(ISDDUsuarioService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<ActionResult<SDDUsuarioReadDto>> Create(
        [FromBody] SDDUsuarioCreateDto dto,
        CancellationToken cancellationToken)
    {
        var read = await _service
            .CreateAsync(dto, User.IsInRole("Admin"), cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { version = "1.0", id = read.id }, read);
    }

    [HttpPut("{id:int}")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<ActionResult<SDDUsuarioReadDto>> Update(
        [FromRoute] int id,
        [FromBody] SDDUsuarioUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var read = await _service
            .UpdateAsync(id, dto, User.IsInRole("Admin"), cancellationToken).ConfigureAwait(false);
        return Ok(read);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SDDUsuarioReadDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _service
            .ListAsync(new PaginationQuery(page, pageSize), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SDDUsuarioReadDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var read = await _service.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return Ok(read);
    }

    [HttpDelete("{id:int}")]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromQuery] string rowVersion,
        CancellationToken cancellationToken)
    {
        if (!RowVersionCodec.TryDecode(rowVersion, out var version))
        {
            throw new BadHttpRequestException("RowVersion must be valid base64.");
        }

        await _service.DeleteAsync(id, version, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
