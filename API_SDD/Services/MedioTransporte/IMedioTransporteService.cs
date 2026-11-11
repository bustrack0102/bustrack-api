using ApiSDD.Dtos.MedioTransporte;

namespace ApiSDD.Services.MedioTransporte;

/// <summary>
/// Contrato de Medios de Transporte (EP-EM-09 a EP-EM-12).
/// </summary>
public interface IMedioTransporteService
{
    Task<IReadOnlyList<MedioTransporteItemDto>> ListAsync(bool? activo, string? municipio, bool? conRutasActivas, CancellationToken cancellationToken);

    Task<MedioTransporteItemDto> CreateAsync(MedioTransporteCreateDto dto, CancellationToken cancellationToken);

    Task<MedioTransporteItemDto> UpdateAsync(int id, MedioTransporteUpdateDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
