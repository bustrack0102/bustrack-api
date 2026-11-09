// ---------------------------------------------------------------------------
// Implementación del servicio de Empresas. Usa MongoDbContext y caché distribuida.
// ---------------------------------------------------------------------------
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using ApiSDD.Data;
using ApiSDD.Dtos.Common;
using ApiSDD.Dtos.Empresa;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace ApiSDD.Services.Empresa;

public sealed class EmpresaService : IEmpresaService
{
    private readonly MongoDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private const string CachePrefix = "empresas:";

    public EmpresaService(MongoDbContext dbContext, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    // ---------------------------------------------------------------------
    // LISTADO (paginado + filtros multicriterio)
    // ---------------------------------------------------------------------
    public async Task<PagedResult<EmpresaListDto>> SearchAsync(EmpresaQueryDto query)
    {
        ArgumentNullException.ThrowIfNull(query);
        // Construir una clave de caché basada en los parámetros.
        var cacheKey = CachePrefix + "list:" + SerializeKey(query);
        var cached = await _cache.GetStringAsync(cacheKey).ConfigureAwait(false);
        if (cached != null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<PagedResult<EmpresaListDto>>(cached)!;
        }

        var filterBuilder = Builders<EmpresaDocument>.Filter;
        var filters = new List<FilterDefinition<EmpresaDocument>>();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            filters.Add(filterBuilder.Text(query.Q));
        }
        if (!string.IsNullOrWhiteSpace(query.MedioTransporteId))
        {
            filters.Add(filterBuilder.Eq("mediosTransporte", query.MedioTransporteId));
        }
        if (!string.IsNullOrWhiteSpace(query.Municipio))
        {
            filters.Add(filterBuilder.Eq(e => e.MunicipioBase, query.Municipio));
        }
        if (query.CalificacionMin.HasValue)
        {
            filters.Add(filterBuilder.Gte(e => e.CalificacionPromedio, query.CalificacionMin.Value));
        }
        if (query.Activo.HasValue)
        {
            filters.Add(filterBuilder.Eq(e => e.Activo, query.Activo.Value));
        }

        var filter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

        // Ordenación (comparación ordinal, sin CA1308).
        var sortBuilder = Builders<EmpresaDocument>.Sort;
        var orden = query.OrdenarPor;
        var sort = string.Equals(orden, "nombre", System.StringComparison.OrdinalIgnoreCase)
            ? sortBuilder.Ascending(e => e.Nombre)
            : sortBuilder.Descending(e => e.TotalRutasActivas);
        if (!string.Equals(orden, "nombre", System.StringComparison.OrdinalIgnoreCase)
            && !string.Equals(orden, "rutas", System.StringComparison.OrdinalIgnoreCase))
        {
            sort = sortBuilder.Descending(e => e.CalificacionPromedio);
        }

        var total = await _dbContext.Empresas.CountDocumentsAsync(filter).ConfigureAwait(false);
        var skip = (query.Pagina - 1) * query.TamanoPagina;
        var list = await _dbContext.Empresas
            .Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(query.TamanoPagina)
            .Project<EmpresaListDto>(Builders<EmpresaDocument>.Projection
                .Include(e => e.Id)
                .Include(e => e.Nombre)
                .Include(e => e.Siglas)
                .Include(e => e.Descripcion)
                .Include(e => e.MunicipioBase)
                .Include(e => e.CalificacionPromedio)
                .Include(e => e.TotalEvaluaciones)
                .Include(e => e.MediosTransporte)
                .Include(e => e.TotalRutasActivas)
                .Include(e => e.FechaActualizacion))
            .ToListAsync().ConfigureAwait(false);

        var result = new PagedResult<EmpresaListDto>(list, (int)total, query.Pagina, query.TamanoPagina);
        // Cache 60 s
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = System.TimeSpan.FromSeconds(60) }).ConfigureAwait(false);
        return result;
    }

    // ---------------------------------------------------------------------
    // AUTOCOMPLETE (texto ligero, máximo 10)
    // ---------------------------------------------------------------------
    public async Task<IReadOnlyList<EmpresaAutocompleteDto>> AutocompleteAsync(string q, int limit = 10)
    {
        var cacheKey = CachePrefix + "ac:" + q + ":" + limit;
        var cached = await _cache.GetStringAsync(cacheKey).ConfigureAwait(false);
        if (cached != null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<EmpresaAutocompleteDto>>(cached)!;
        }

        var filter = Builders<EmpresaDocument>.Filter.Text(q);
        var projection = Builders<EmpresaDocument>.Projection
            .Include(e => e.Id)
            .Include(e => e.Nombre)
            .Include(e => e.Siglas)
            .Include(e => e.MunicipioBase)
            .Include(e => e.CalificacionPromedio);

        var list = await _dbContext.Empresas
            .Find(filter)
            .Limit(limit)
            .Project<EmpresaAutocompleteDto>(projection)
            .ToListAsync().ConfigureAwait(false);

        var json = System.Text.Json.JsonSerializer.Serialize(list);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = System.TimeSpan.FromSeconds(300) }).ConfigureAwait(false);
        return list;
    }

    // ---------------------------------------------------------------------
    // DETALLE (por Id)
    // ---------------------------------------------------------------------
    public async Task<EmpresaDetailDto?> GetDetailAsync(string id)
    {
        var cacheKey = CachePrefix + "detail:" + id;
        var cached = await _cache.GetStringAsync(cacheKey).ConfigureAwait(false);
        if (cached != null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<EmpresaDetailDto>(cached)!;
        }

        var filter = Builders<EmpresaDocument>.Filter.Eq(e => e.Id, id);
        var empresa = await _dbContext.Empresas.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        if (empresa == null || !empresa.Activo)
        {
            return null;
        }

        // Obtener rutas asociadas (solo los campos ligeros necesarios)
        var rutas = await _dbContext.Rutas
            .Find(r => r.EmpresaId == empresa.Id)
            .Project<RutaDto>(Builders<RutaDocument>.Projection
                .Include(r => r.Id)
                .Include(r => r.MedioTransporteId)
                .Include(r => r.EmpresaId))
            .ToListAsync().ConfigureAwait(false);

        var detalle = new EmpresaDetailDto
        {
            Id = empresa.Id,
            Nombre = empresa.Nombre,
            Siglas = empresa.Siglas,
            Descripcion = empresa.Descripcion,
            TelefonoContacto = "",
            SitioWeb = "",
            HorarioAtencion = "",
            CalificacionPromedio = empresa.CalificacionPromedio,
            TotalEvaluaciones = empresa.TotalEvaluaciones,
            MediosTransporte = empresa.MediosTransporte,
            Rutas = rutas,
            DestinosAsociados = Array.Empty<DestinoDto>(), // placeholder; ampliar si se necesita.
            FechaUltimaActualizacion = empresa.FechaActualizacion
        };

        var json = System.Text.Json.JsonSerializer.Serialize(detalle);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = System.TimeSpan.FromSeconds(600) }).ConfigureAwait(false);
        return detalle;
    }

    // Helper para generar una cadena de caché única
    private static string SerializeKey(EmpresaQueryDto q) =>
        $"q={q.Q ?? ""}&mt={q.MedioTransporteId ?? ""}&mun={q.Municipio ?? ""}&cmin={(q.CalificacionMin?.ToString(CultureInfo.InvariantCulture) ?? "")}&act={q.Activo}&ord={q.OrdenarPor}&p={q.Pagina}&s={q.TamanoPagina}";
}
