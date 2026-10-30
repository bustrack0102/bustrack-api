using ApiSDD.Dtos;

// Service contract follows the auth slice it owns.
namespace ApiSDD.Services;

/// <summary>
/// nv-05: login slice contract. Emits JWT after Argon2id verification with lockout.
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken);
}
