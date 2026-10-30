namespace ApiSDD.Common;

/// <summary>
/// Página uniforme nv-02 (constitución §4). Wire legacy PascalCase preservado
/// con <c>PropertyNamingPolicy=null</c>.
/// </summary>
/// <typeparam name="T">Tipo del item paginado.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
