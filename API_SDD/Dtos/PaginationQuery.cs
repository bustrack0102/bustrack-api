namespace ApiSDD.Dtos;

/// <summary>
/// nv-02: parámetros de paginación. DTO sin entidad ni tabla; demuestra el molde
/// DTO→Validator espejo. Wire legacy PascalCase.
/// </summary>
public sealed record PaginationQuery(int Page, int PageSize);
