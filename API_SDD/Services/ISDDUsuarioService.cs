using ApiSDD.Common;
using ApiSDD.Dtos;

// Service name follows the SDDUsuario entity it owns; Sonar S101 cannot apply.
#pragma warning disable S101

namespace ApiSDD.Services;

/// <summary>
/// nv-04: user slice contract. No entity caching; concurrency via RowVersion.
/// </summary>
public interface ISDDUsuarioService
{
    Task<SDDUsuarioReadDto> CreateAsync(SDDUsuarioCreateDto dto, bool isAdmin, CancellationToken cancellationToken);

    Task<SDDUsuarioReadDto> UpdateAsync(int id, SDDUsuarioUpdateDto dto, bool isAdmin, CancellationToken cancellationToken);

    Task<PagedResult<SDDUsuarioReadDto>> ListAsync(PaginationQuery query, CancellationToken cancellationToken);

    Task<SDDUsuarioReadDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);
}
