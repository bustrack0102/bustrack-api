using ApiSDD.Dtos.Common;
using ApiSDD.Dtos.Empresa;

namespace ApiSDD.Services.Empresa;

/// <summary>
/// Contrato SQL para el dominio de Empresas (EP-EM-01 a EP-EM-08).
/// </summary>
public interface IEmpresaSqlService
{
    Task<PagedResult<EmpresaListDto>> SearchAsync(EmpresaQueryDto query, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmpresaAutocompleteDto>> AutocompleteAsync(string q, int limite, CancellationToken cancellationToken);

    Task<EmpresaDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmpresaRutaItemDto>> GetRutasAsync(Guid id, int? medioTransporteId, string? paradaId, string? diaOperacion, CancellationToken cancellationToken);

    Task<EmpresaCompararResponseDto> CompararAsync(string? paradaOrigenId, string? paradaDestinoId, string? destinoTuristicoId, string? empresasIds, CancellationToken cancellationToken);

    Task<EmpresaResponseDto> CreateAsync(EmpresaCreateDto dto, CancellationToken cancellationToken);

    Task<EmpresaResponseDto> UpdateAsync(Guid id, EmpresaUpdateDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
