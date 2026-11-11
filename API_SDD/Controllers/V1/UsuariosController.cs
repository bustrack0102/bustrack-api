using ApiSDD.Dtos;
using ApiSDD.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

#pragma warning disable S101

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Endpoints de perfil y gestión de usuarios (EP-AU-07 a EP-AU-10).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioPerfilService _perfilService;

    public UsuariosController(IUsuarioPerfilService perfilService)
    {
        _perfilService = perfilService ?? throw new ArgumentNullException(nameof(perfilService));
    }

    // EP-AU-07: Perfil de usuario autenticado
    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<ActionResult<UsuarioMeResponseDto>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _perfilService.GetMeAsync(User, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // EP-AU-08: Edición de perfil propio
    [HttpPut("me")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<ActionResult<UsuarioMeUpdateResponseDto>> UpdateMe(
        [FromBody] UsuarioMeUpdateRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = await _perfilService.UpdateMeAsync(User, dto, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // EP-AU-09: Baja de cuenta propia
    [HttpDelete("me")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> DeleteMe(
        [FromBody] UsuarioMeDeleteRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = await _perfilService.DeleteMeAsync(User, dto, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    // EP-AU-10: Listado administrativo con filtros multicriterio
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    [EnableRateLimiting("Admin")]
    public async Task<ActionResult<PagedUsuariosAdminResponseDto>> List(
        [FromQuery] string? q = null,
        [FromQuery] string? rol = null,
        [FromQuery] bool? activo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] string? ordenarPor = "fechaCreacion",
        [FromQuery] string? direccion = "desc",
        CancellationToken cancellationToken = default)
    {
        var result = await _perfilService.ListAdminAsync(
            q,
            rol,
            activo,
            pagina,
            tamanoPagina,
            ordenarPor,
            direccion,
            cancellationToken).ConfigureAwait(false);

        return Ok(result);
    }
}
