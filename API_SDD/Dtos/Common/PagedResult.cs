// ---------------------------------------------------------------------------
// Generic pagination wrapper used by services.
// ---------------------------------------------------------------------------
using System.Collections.Generic;

namespace ApiSDD.Dtos.Common;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas => (int)System.Math.Ceiling(TotalRegistros / (double)TamanoPagina);
    public bool TienePaginaAnterior => Pagina > 1;
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalRegistros, int pagina, int tamanoPagina)
    {
        Items = items;
        TotalRegistros = totalRegistros;
        Pagina = pagina;
        TamanoPagina = tamanoPagina;
    }
}

