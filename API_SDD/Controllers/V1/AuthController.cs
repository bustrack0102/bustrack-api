using ApiSDD.Dtos;
using ApiSDD.Security;
using ApiSDD.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

#pragma warning disable S101, S6960

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Endpoints de autenticación (NV-05, NV-06 y EP-AU-01 a EP-AU-06).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    private readonly ITokenRevocationService _revocation;

    public AuthController(IAuthService service, ITokenRevocationService revocation)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _revocation = revocation ?? throw new ArgumentNullException(nameof(revocation));
    }

    // EP-AU-01: Registro de usuario
    [HttpPost("registro")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> Registro(
        [FromBody] RegistroRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = await _service.RegistroAsync(dto, cancellationToken).ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    // EP-AU-02 + NV-05: Login con soporte unificado
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginInputDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (!string.IsNullOrWhiteSpace(dto.strNombreUsuario))
        {
            var legacy = await _service.LoginAsync(
                new LoginRequestDto(dto.strNombreUsuario, dto.strPWD ?? dto.password ?? string.Empty),
                cancellationToken).ConfigureAwait(false);
            return Ok(legacy);
        }

        var response = await _service.LoginV2Async(dto, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    // EP-AU-03: Refresh Token con rotación
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = await _service.RefreshAsync(dto, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    // EP-AU-04 + NV-06: Logout con revocación de tokens
    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto? dto,
        CancellationToken cancellationToken)
    {
        if (dto is not null && (!string.IsNullOrWhiteSpace(dto.refreshToken) || dto.cerrarTodosLosDispositivos))
        {
            var res = await _service.LogoutV2Async(User, dto, cancellationToken).ConfigureAwait(false);
            return Ok(res);
        }

        await _revocation.RevokeCurrentAsync(User, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    // EP-AU-05: Recuperación de contraseña
    [HttpPost("recuperar-password")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> RecuperarPassword(
        [FromBody] RecuperarPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = await _service.RecuperarPasswordAsync(dto, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    // EP-AU-06: Restablecimiento de contraseña
    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> RestablecerPassword(
        [FromBody] RestablecerPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var response = await _service.RestablecerPasswordAsync(dto, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
