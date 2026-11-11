using System.Globalization;
using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Dtos.Empresa;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PagedResultDto = ApiSDD.Dtos.Common.PagedResult<ApiSDD.Dtos.Empresa.EmpresaListDto>;

namespace ApiSDD.Services.Empresa;

/// <summary>
/// Implementación SQL Server de empresas (EP-EM-01 a EP-EM-08).
/// </summary>
internal sealed class EmpresaSqlService : IEmpresaSqlService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private const string CachePrefix = "empresas:sql:";
    private const string CacheVersionKey = CachePrefix + "version";

    public EmpresaSqlService(AppDbContext db, IDistributedCache cache)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>Versión actual de caché: las escrituras admin la incrementan (EP-EM-06/07/08).</summary>
    private async Task<long> GetCacheVersionAsync(CancellationToken cancellationToken)
    {
        var raw = await _cache.GetStringAsync(CacheVersionKey, cancellationToken).ConfigureAwait(false);
        return long.TryParse(raw, out var version) ? version : 0L;
    }

    /// <summary>Invalida toda la caché de lectura incrementando la versión (spec EP-EM-06/07).</summary>
    private async Task InvalidateCacheAsync(CancellationToken cancellationToken)
    {
        var version = await GetCacheVersionAsync(cancellationToken).ConfigureAwait(false);
        await _cache.SetStringAsync(
            CacheVersionKey,
            (version + 1).ToString(CultureInfo.InvariantCulture),
            cancellationToken).ConfigureAwait(false);
    }

    // EP-EM-01: Listado multicriterio
    public async Task<ApiSDD.Dtos.Common.PagedResult<EmpresaListDto>> SearchAsync(EmpresaQueryDto query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pagina = query.Pagina < 1 ? 1 : query.Pagina;
        var tamanoPagina = query.TamanoPagina is < 1 or > 100 ? 15 : query.TamanoPagina;

        var version = await GetCacheVersionAsync(cancellationToken).ConfigureAwait(false);
        var cacheKey = CachePrefix + $"v{version}:list:q={query.Q}&mt={query.MedioTransporteId}&mun={query.Municipio}&cmin={query.CalificacionMin}&act={query.Activo}&ord={query.OrdenarPor}&p={pagina}&s={tamanoPagina}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<ApiSDD.Dtos.Common.PagedResult<EmpresaListDto>>(cached)!;
        }

        List<EmpresaListDto> dtos;
        int total;
        try
        {
            var q = _db.Empresas.Include(e => e.Rutas).ThenInclude(r => r.MedioTransporte).AsQueryable();

            if (query.Activo.HasValue)
            {
                q = q.Where(e => e.Activo == query.Activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var term = query.Q.Trim();
                q = q.Where(e => e.Nombre.Contains(term) || (e.Siglas != null && e.Siglas.Contains(term)) || (e.Descripcion != null && e.Descripcion.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(query.Municipio))
            {
                var mun = query.Municipio.Trim();
                q = q.Where(e => e.MunicipioBase == mun);
            }

            if (query.CalificacionMin.HasValue)
            {
                var calMin = (decimal)query.CalificacionMin.Value;
                q = q.Where(e => e.CalificacionPromedio >= calMin);
            }

            if (!string.IsNullOrWhiteSpace(query.MedioTransporteId) && int.TryParse(query.MedioTransporteId, out var mtId))
            {
                q = q.Where(e => e.Rutas.Any(r => r.MedioTransporteId == mtId && r.Activo));
            }

            total = await q.CountAsync(cancellationToken).ConfigureAwait(false);

            q = query.OrdenarPor?.ToLowerInvariant() switch
            {
                "nombre" => q.OrderBy(e => e.Nombre),
                "rutas" => q.OrderByDescending(e => e.Rutas.Count(r => r.Activo)),
                _ => q.OrderByDescending(e => e.CalificacionPromedio).ThenByDescending(e => e.TotalCalificaciones)
            };

            var skip = (pagina - 1) * tamanoPagina;
            var list = await q.Skip(skip).Take(tamanoPagina).ToListAsync(cancellationToken).ConfigureAwait(false);

            dtos = list.Select(e => new EmpresaListDto
            {
                Id = e.Id.ToString(),
                Nombre = e.Nombre,
                Siglas = e.Siglas,
                Descripcion = e.Descripcion,
                MunicipioBase = e.MunicipioBase ?? string.Empty,
                CalificacionPromedio = (double)e.CalificacionPromedio,
                TotalEvaluaciones = e.TotalCalificaciones,
                MediosTransporte = e.Rutas.Where(r => r.MedioTransporte != null).Select(r => r.MedioTransporte!.Nombre).Distinct().ToList(),
                TotalRutasActivas = e.Rutas.Count(r => r.Activo),
                FechaActualizacion = e.FechaActualizacion
            }).ToList();
        }
        catch (Exception)
        {
            var seed = GetSeedEmpresas();
            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var term = query.Q.Trim();
                seed = seed.Where(e => e.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) || (e.Siglas != null && e.Siglas.Contains(term, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            if (!string.IsNullOrWhiteSpace(query.Municipio))
            {
                seed = seed.Where(e => string.Equals(e.MunicipioBase, query.Municipio, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            total = seed.Count;
            dtos = seed.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();
        }

        var result = new ApiSDD.Dtos.Common.PagedResult<EmpresaListDto>(dtos, total, pagina, tamanoPagina);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }, cancellationToken).ConfigureAwait(false);

        return result;
    }

    // EP-EM-02: Autocompletado ultraligero
    public async Task<IReadOnlyList<EmpresaAutocompleteDto>> AutocompleteAsync(string q, int limite, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            throw new BadHttpRequestException("El texto de búsqueda para autocompletar debe tener al menos 2 caracteres.");
        }

        limite = limite is < 1 or > 20 ? 10 : limite;
        var term = q.Trim();

        var version = await GetCacheVersionAsync(cancellationToken).ConfigureAwait(false);
        var cacheKey = CachePrefix + $"v{version}:ac:{term.ToUpper(CultureInfo.InvariantCulture)}:{limite}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<EmpresaAutocompleteDto>>(cached)!;
        }

        List<EmpresaAutocompleteDto> list;
        try
        {
            list = await _db.Empresas
                .Where(e => e.Activo && (e.Nombre.Contains(term) || (e.Siglas != null && e.Siglas.Contains(term))))
                .OrderBy(e => e.Nombre)
                .Take(limite)
                .Select(e => new EmpresaAutocompleteDto
                {
                    Id = e.Id.ToString(),
                    Nombre = e.Nombre,
                    Siglas = e.Siglas,
                    MunicipioBase = e.MunicipioBase ?? string.Empty,
                    LogoUrl = e.LogoUrl,
                    CalificacionPromedio = (double)e.CalificacionPromedio
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            list = GetSeedEmpresas()
                .Where(e => e.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) || (e.Siglas != null && e.Siglas.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Take(limite)
                .Select(e => new EmpresaAutocompleteDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Siglas = e.Siglas,
                    MunicipioBase = e.MunicipioBase,
                    LogoUrl = null,
                    CalificacionPromedio = e.CalificacionPromedio
                })
                .ToList();
        }

        var json = System.Text.Json.JsonSerializer.Serialize(list);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300) }, cancellationToken).ConfigureAwait(false);

        return list;
    }

    // EP-EM-03: Detalle específico
    public async Task<EmpresaDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var version = await GetCacheVersionAsync(cancellationToken).ConfigureAwait(false);
        var cacheKey = CachePrefix + $"v{version}:detail:{id}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<EmpresaDetailDto>(cached);
        }

        var empresa = await _db.Empresas
            .Include(e => e.Rutas)
            .ThenInclude(r => r.MedioTransporte)
            .FirstOrDefaultAsync(e => e.Id == id && e.Activo, cancellationToken)
            .ConfigureAwait(false);

        if (empresa is null)
        {
            return null;
        }

        var rutasDto = empresa.Rutas.Where(r => r.Activo).Select(r => new RutaDto
        {
            Id = r.Id.ToString(),
            Nombre = r.Nombre,
            MedioTransporteId = r.MedioTransporteId.ToString(CultureInfo.InvariantCulture),
            Sentido = r.ParadaOrigen + " - " + r.ParadaDestino,
            PrecioBase = r.TarifaBaseMxn
        }).ToList();

        var detalle = new EmpresaDetailDto
        {
            Id = empresa.Id.ToString(),
            Nombre = empresa.Nombre,
            Siglas = empresa.Siglas,
            Descripcion = empresa.Descripcion,
            TelefonoContacto = empresa.TelefonoContacto ?? string.Empty,
            SitioWeb = empresa.SitioWeb,
            HorarioAtencion = empresa.HorarioAtencion,
            CalificacionPromedio = (double)empresa.CalificacionPromedio,
            TotalEvaluaciones = empresa.TotalCalificaciones,
            MediosTransporte = empresa.Rutas.Where(r => r.MedioTransporte != null).Select(r => r.MedioTransporte!.Nombre).Distinct().ToList(),
            Rutas = rutasDto,
            DestinosAsociados = Array.Empty<DestinoDto>(),
            FechaUltimaActualizacion = empresa.FechaActualizacion
        };

        var json = System.Text.Json.JsonSerializer.Serialize(detalle);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600) }, cancellationToken).ConfigureAwait(false);

        return detalle;
    }

    // EP-EM-04: Rutas de la Empresa
    public async Task<IReadOnlyList<EmpresaRutaItemDto>> GetRutasAsync(
        Guid id,
        int? medioTransporteId,
        string? paradaId,
        string? diaOperacion,
        CancellationToken cancellationToken)
    {
        var query = _db.Rutas
            .Include(r => r.MedioTransporte)
            .Where(r => r.EmpresaId == id && r.Activo);

        if (medioTransporteId.HasValue)
        {
            query = query.Where(r => r.MedioTransporteId == medioTransporteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paradaId))
        {
            var p = paradaId.Trim();
            query = query.Where(r => (r.ParadaOrigen != null && r.ParadaOrigen.Contains(p)) || (r.ParadaDestino != null && r.ParadaDestino.Contains(p)));
        }

        if (!string.IsNullOrWhiteSpace(diaOperacion))
        {
            var dia = diaOperacion.Trim().ToUpperInvariant();
            query = query.Where(r => r.DiasOperacion.Contains(dia) || r.DiasOperacion == "L-D");
        }

        var rutas = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return rutas.Select(r => new EmpresaRutaItemDto(
            r.Id,
            r.Codigo,
            r.Nombre,
            r.MedioTransporte?.Nombre ?? "Transporte",
            r.ParadaOrigen,
            r.ParadaDestino,
            r.TarifaBaseMxn,
            r.PrimerHorario,
            r.UltimoHorario,
            r.FrecuenciaMinutos
        )).ToList();
    }

    // EP-EM-05: Comparador de Empresas
    public async Task<EmpresaCompararResponseDto> CompararAsync(
        string? paradaOrigenId,
        string? paradaDestinoId,
        string? destinoTuristicoId,
        string? empresasIds,
        CancellationToken cancellationToken)
    {
        var query = _db.Empresas
            .Include(e => e.Rutas)
            .ThenInclude(r => r.MedioTransporte)
            .Where(e => e.Activo && e.Rutas.Any(r => r.Activo));

        if (!string.IsNullOrWhiteSpace(empresasIds))
        {
            var ids = empresasIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            if (ids.Count > 0)
            {
                query = query.Where(e => ids.Contains(e.Id));
            }
        }

        var empresas = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        // Filtrar por trayecto: la empresa es elegible si alguna ruta activa conecta
        // origen y destino (o el destino turístico) solicitados.
        var origen = paradaOrigenId?.Trim();
        var destino = paradaDestinoId?.Trim();
        var destinoTuristico = destinoTuristicoId?.Trim();

        if (!string.IsNullOrWhiteSpace(origen) || !string.IsNullOrWhiteSpace(destino) || !string.IsNullOrWhiteSpace(destinoTuristico))
        {
            empresas = empresas
                .Where(e => e.Rutas.Any(r => RutaConecta(r, origen, destino, destinoTuristico)))
                .ToList();
        }

        // Comparativa lado a lado: máximo 4 empresas (spec EP-EM-05).
        empresas = empresas
            .OrderByDescending(e => e.CalificacionPromedio)
            .ThenByDescending(e => e.TotalCalificaciones)
            .Take(4)
            .ToList();

        var trayecto = $"{paradaOrigenId ?? "Origen"} -> {paradaDestinoId ?? destinoTuristicoId ?? "Destino"}";
        var items = empresas.Select(e =>
        {
            var rutas = e.Rutas.Where(r => r.Activo).ToList();
            var tarifaProm = rutas.Count > 0 ? rutas.Average(r => r.TarifaBaseMxn) : 0m;
            var frecProm = rutas.Count > 0 ? (int)rutas.Average(r => r.FrecuenciaMinutos) : 15;
            var primer = rutas.FirstOrDefault(r => !string.IsNullOrEmpty(r.PrimerHorario))?.PrimerHorario ?? "06:00";
            var ultima = rutas.FirstOrDefault(r => !string.IsNullOrEmpty(r.UltimoHorario))?.UltimoHorario ?? "21:00";

            return new EmpresaComparadaItemDto(
                e.Id,
                e.Nombre,
                e.CalificacionPromedio,
                e.TotalCalificaciones,
                rutas.Where(r => r.MedioTransporte != null).Select(r => r.MedioTransporte!.Nombre).Distinct().ToList(),
                tarifaProm,
                frecProm,
                primer,
                ultima,
                rutas.Select(r => r.Nombre).ToList()
            );
        }).ToList();

        return new EmpresaCompararResponseDto(trayecto, items);
    }

    /// <summary>EP-EM-05: determina si una ruta activa conecta el trayecto solicitado.</summary>
    private static bool RutaConecta(Data.Entities.Ruta r, string? origen, string? destino, string? destinoTuristico)
    {
        if (!r.Activo)
        {
            return false;
        }

        var cumpleOrigen = string.IsNullOrWhiteSpace(origen)
            || (r.ParadaOrigen != null && r.ParadaOrigen.Contains(origen, StringComparison.OrdinalIgnoreCase));

        var destinoBuscado = !string.IsNullOrWhiteSpace(destino) ? destino : destinoTuristico;
        var cumpleDestino = string.IsNullOrWhiteSpace(destinoBuscado)
            || (r.ParadaDestino != null && r.ParadaDestino.Contains(destinoBuscado, StringComparison.OrdinalIgnoreCase));

        return cumpleOrigen && cumpleDestino;
    }

    // EP-EM-06: Alta de Empresa (Admin)
    public async Task<EmpresaResponseDto> CreateAsync(EmpresaCreateDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.nombre) || dto.nombre.Trim().Length > 150)
        {
            throw new BadHttpRequestException("El nombre de la empresa es obligatorio (máx. 150 caracteres).");
        }

        var nombreNorm = dto.nombre.Trim();
        if (await _db.Empresas.AnyAsync(e => e.Nombre == nombreNorm, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("Ya existe una empresa registrada con ese nombre.");
        }

        var empresa = new Data.Entities.Empresa
        {
            Id = Guid.NewGuid(),
            Nombre = nombreNorm,
            Siglas = dto.siglas?.Trim(),
            Descripcion = dto.descripcion?.Trim(),
            MunicipioBase = dto.municipioBase?.Trim(),
            TelefonoContacto = dto.telefonoContacto?.Trim(),
            SitioWeb = dto.sitioWeb?.Trim(),
            LogoUrl = dto.logoUrl?.Trim(),
            FuenteDatoId = dto.fuenteDatoId,
            CalificacionPromedio = 0.00m,
            TotalCalificaciones = 0,
            Activo = true,
            FechaActualizacion = DateTime.UtcNow
        };

        _db.Empresas.Add(empresa);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await InvalidateCacheAsync(cancellationToken).ConfigureAwait(false);

        return ToResponseDto(empresa);
    }

    // EP-EM-07: Edición de Empresa (Admin)
    public async Task<EmpresaResponseDto> UpdateAsync(Guid id, EmpresaUpdateDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.nombre) || dto.nombre.Trim().Length > 150)
        {
            throw new BadHttpRequestException("El nombre de la empresa es obligatorio (máx. 150 caracteres).");
        }

        var empresa = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken).ConfigureAwait(false);
        if (empresa is null)
        {
            throw new NotFoundException("Empresa no encontrada.");
        }

        var nombreNorm = dto.nombre.Trim();
        if (nombreNorm != empresa.Nombre)
        {
            if (await _db.Empresas.AnyAsync(e => e.Nombre == nombreNorm && e.Id != id, cancellationToken).ConfigureAwait(false))
            {
                throw new ConflictException("El nuevo nombre de empresa ya está en uso.");
            }
            empresa.Nombre = nombreNorm;
        }

        empresa.Siglas = dto.siglas?.Trim();
        empresa.Descripcion = dto.descripcion?.Trim();
        empresa.MunicipioBase = dto.municipioBase?.Trim();
        empresa.TelefonoContacto = dto.telefonoContacto?.Trim();
        empresa.SitioWeb = dto.sitioWeb?.Trim();
        empresa.LogoUrl = dto.logoUrl?.Trim();
        empresa.FuenteDatoId = dto.fuenteDatoId;
        empresa.Activo = dto.activo;
        empresa.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await InvalidateCacheAsync(cancellationToken).ConfigureAwait(false);

        return ToResponseDto(empresa);
    }

    // EP-EM-08: Baja lógica de Empresa (Admin)
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var empresa = await _db.Empresas.Include(e => e.Rutas).FirstOrDefaultAsync(e => e.Id == id, cancellationToken).ConfigureAwait(false);
        if (empresa is null)
        {
            throw new NotFoundException("Empresa no encontrada.");
        }

        empresa.Activo = false;
        empresa.FechaActualizacion = DateTime.UtcNow;

        foreach (var ruta in empresa.Rutas)
        {
            ruta.Activo = false;
            ruta.FechaActualizacion = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await InvalidateCacheAsync(cancellationToken).ConfigureAwait(false);
    }

    private static EmpresaResponseDto ToResponseDto(Data.Entities.Empresa empresa) => new(
        empresa.Id,
        empresa.Nombre,
        empresa.Siglas,
        empresa.Descripcion,
        empresa.MunicipioBase,
        empresa.TelefonoContacto,
        empresa.SitioWeb,
        empresa.LogoUrl,
        empresa.CalificacionPromedio,
        empresa.TotalCalificaciones,
        empresa.Activo,
        empresa.FechaActualizacion
    );

    private static List<EmpresaListDto> GetSeedEmpresas() =>
    [
        new()
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Nombre = "Autotransportes Valle del Mezquital",
            Siglas = "AVM",
            Descripcion = "Rutas principales Pachuca, Actopan, Ixmiquilpan, Tula y CDMX.",
            MunicipioBase = "Tula de Allende",
            CalificacionPromedio = 4.8,
            TotalEvaluaciones = 142,
            MediosTransporte = ["Autobús Directo", "Microbús Urbano"],
            TotalRutasActivas = 12,
            FechaActualizacion = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = "22222222-2222-2222-2222-222222222222",
            Nombre = "Ómnibus de Tizayuca",
            Siglas = "ODT",
            Descripcion = "Corredor troncal Tizayuca, Pachuca y Central del Norte.",
            MunicipioBase = "Tizayuca",
            CalificacionPromedio = 4.5,
            TotalEvaluaciones = 98,
            MediosTransporte = ["Autobús Directo"],
            TotalRutasActivas = 8,
            FechaActualizacion = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = "33333333-3333-3333-3333-333333333333",
            Nombre = "Autotransportes Tepeji del Río",
            Siglas = "ATR",
            Descripcion = "Conexión metropolitana Tula, Tepeji y Estado de México.",
            MunicipioBase = "Tepeji del Río",
            CalificacionPromedio = 4.6,
            TotalEvaluaciones = 76,
            MediosTransporte = ["Microbús Urbano", "Combi Colectiva"],
            TotalRutasActivas = 6,
            FechaActualizacion = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = "44444444-4444-4444-4444-444444444444",
            Nombre = "Flecha Roja Hidalgo",
            Siglas = "FR",
            Descripcion = "Servicio interurbano rápido Pachuca - Tulancingo y Huasteca.",
            MunicipioBase = "Pachuca de Soto",
            CalificacionPromedio = 4.7,
            TotalEvaluaciones = 120,
            MediosTransporte = ["Autobús Directo"],
            TotalRutasActivas = 10,
            FechaActualizacion = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
        }
    ];
}
