using System.Security.Claims;
using ApiSDD.Dtos;

namespace ApiSDD.Services;

/// <summary>
/// Contrato de autenticación (NV-05 + EP-AU-01 a EP-AU-06).
/// </summary>
public interface IAuthService
{
    // Legacy NV-05
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-01: Registro
    Task<AuthResponseDto> RegistroAsync(RegistroRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-02: Login con Refresh Token
    Task<AuthResponseDto> LoginV2Async(LoginInputDto dto, CancellationToken cancellationToken);

    // EP-AU-03: Refresh con rotación
    Task<TokenRefreshResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-04: Logout con revocación de refresh tokens
    Task<MessageResponseDto> LogoutV2Async(ClaimsPrincipal userPrincipal, LogoutRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-05: Recuperación de contraseña
    Task<MessageResponseDto> RecuperarPasswordAsync(RecuperarPasswordRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-06: Restablecimiento de contraseña
    Task<MessageResponseDto> RestablecerPasswordAsync(RestablecerPasswordRequestDto dto, CancellationToken cancellationToken);
}
