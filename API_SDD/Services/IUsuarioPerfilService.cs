using System.Security.Claims;
using ApiSDD.Dtos;

namespace ApiSDD.Services;

/// <summary>
/// Contrato para gestión de perfil y administración de usuarios (EP-AU-07 a EP-AU-10).
/// </summary>
public interface IUsuarioPerfilService
{
    // EP-AU-07: Perfil de usuario autenticado
    Task<UsuarioMeResponseDto> GetMeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);

    // EP-AU-08: Edición de perfil propio
    Task<UsuarioMeUpdateResponseDto> UpdateMeAsync(ClaimsPrincipal principal, UsuarioMeUpdateRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-09: Baja de cuenta
    Task<MessageResponseDto> DeleteMeAsync(ClaimsPrincipal principal, UsuarioMeDeleteRequestDto dto, CancellationToken cancellationToken);

    // EP-AU-10: Listado administrativo multicriterio
    Task<PagedUsuariosAdminResponseDto> ListAdminAsync(
        string? q,
        string? rol,
        bool? activo,
        int pagina,
        int tamanoPagina,
        string? ordenarPor,
        string? direccion,
        CancellationToken cancellationToken);
}
