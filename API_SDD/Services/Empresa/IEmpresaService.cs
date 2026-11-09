// ---------------------------------------------------------------------------
// Interface del servicio de Empresas.
// ---------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiSDD.Dtos.Empresa;
using ApiSDD.Dtos.Common;

namespace ApiSDD.Services.Empresa;

public interface IEmpresaService
{
    Task<PagedResult<EmpresaListDto>> SearchAsync(EmpresaQueryDto query);
    Task<IReadOnlyList<EmpresaAutocompleteDto>> AutocompleteAsync(string q, int limit = 10);
    Task<EmpresaDetailDto?> GetDetailAsync(string id);
}

