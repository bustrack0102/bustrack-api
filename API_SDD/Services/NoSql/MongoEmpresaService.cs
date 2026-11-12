using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Empresa;
using ApiSDD.Services.Empresa;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Implementación NoSQL de Empresas (EP-EM-01 a EP-EM-08) con caché Redis versionado y comparativa real.
/// </summary>
internal sealed class MongoEmpresaService : IEmpresaSqlService
{
    private const string CachePrefix = "empresas:nosql:";
    private const string CacheVersionKey = CachePrefix + "version";

    private readonly IBusTrackNoSqlStore _store;
    private readonly IDistributedCache _cache;

    public MongoEmpresaService(IBusTrackNoSqlStore store, IDistributedCache cache)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Dtos.Common.PagedResult<EmpresaListDto>> SearchAsync(
        EmpresaQueryDto query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var p = query.Pagina < 1 ? 1 : query.Pagina;
        var s = query.TamanoPagina is < 1 or > 100 ? 15 : query.TamanoPagina;
        var version = await DistributedCacheHelper.GetVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        var key = $"{CachePrefix}v{version}:list:{query.Q}:{query.MedioTransporteId}:{query.Municipio}:{query.CalificacionMin}:{query.Activo}:{query.OrdenarPor}:{p}:{s}";

        return await DistributedCacheHelper.GetOrSetAsync(_cache, key, TimeSpan.FromSeconds(60), async () =>
        {
            var empresas = await _store.ListEmpresasAsync(cancellationToken).ConfigureAwait(false);
            var rutas = await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false);
            var list = empresas.AsEnumerable();

            if (query.Activo.HasValue)
            {
                list = list.Where(e => e.Activo == query.Activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var term = query.Q.Trim();
                list = list.Where(e =>
                    e.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (e.Siglas?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (e.Descripcion?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(query.Municipio))
            {
                var mun = query.Municipio.Trim();
                list = list.Where(e => e.MunicipioBase.Contains(mun, StringComparison.OrdinalIgnoreCase));
            }

            if (query.CalificacionMin.HasValue)
            {
                list = list.Where(e => e.CalificacionPromedio >= query.CalificacionMin.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.MedioTransporteId) && int.TryParse(query.MedioTransporteId, out var mtId))
            {
                var empresasConMt = rutas.Where(r => r.Activa && r.MedioTransporteId == mtId).Select(r => r.EmpresaId).ToHashSet();
                list = list.Where(e => empresasConMt.Contains(e.Id));
            }

            list = (query.OrdenarPor ?? "nombre").ToLowerInvariant() switch
            {
                "calificacion" => list.OrderByDescending(e => e.CalificacionPromedio),
                "rutas" => list.OrderByDescending(e => rutas.Count(r => r.Activa && r.EmpresaId == e.Id)),
                _ => list.OrderBy(e => e.Nombre),
            };

            var materialized = list.ToList();
            var dtos = materialized.Skip((p - 1) * s).Take(s).Select(e => new EmpresaListDto
            {
                Id = e.Id.ToString(),
                Nombre = e.Nombre,
                Siglas = e.Siglas,
                Descripcion = e.Descripcion,
                MunicipioBase = e.MunicipioBase,
                CalificacionPromedio = e.CalificacionPromedio,
                TotalEvaluaciones = e.TotalEvaluaciones,
                MediosTransporte = e.MediosTransporte,
                TotalRutasActivas = rutas.Count(r => r.Activa && r.EmpresaId == e.Id),
                FechaActualizacion = e.FechaActualizacion,
            }).ToList();

            return new Dtos.Common.PagedResult<EmpresaListDto>(dtos, materialized.Count, p, s);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<EmpresaAutocompleteDto>> AutocompleteAsync(
        string q,
        int limite,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return [];
        }

        var max = Math.Clamp(limite, 1, 20);
        var version = await DistributedCacheHelper.GetVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        var key = $"{CachePrefix}v{version}:ac:{q.Trim().ToLowerInvariant()}:{max}";

        return await DistributedCacheHelper.GetOrSetAsync(_cache, key, TimeSpan.FromSeconds(180), async () =>
        {
            var term = q.Trim();
            var empresas = await _store.ListEmpresasAsync(cancellationToken).ConfigureAwait(false);
            return (IReadOnlyList<EmpresaAutocompleteDto>)empresas
                .Where(e => e.Activo && (e.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (e.Siglas?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
                .OrderBy(e => e.Nombre)
                .Take(max)
                .Select(e => new EmpresaAutocompleteDto
                {
                    Id = e.Id.ToString(),
                    Nombre = e.Nombre,
                    Siglas = e.Siglas,
                    MunicipioBase = e.MunicipioBase,
                    LogoUrl = e.LogoUrl,
                    CalificacionPromedio = e.CalificacionPromedio,
                })
                .ToList();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EmpresaDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var version = await DistributedCacheHelper.GetVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        var key = $"{CachePrefix}v{version}:detail:{id}";

        return await DistributedCacheHelper.GetOrSetAsync<EmpresaDetailDto?>(_cache, key, TimeSpan.FromSeconds(300), async () =>
        {
            var emp = await _store.GetEmpresaByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (emp is null || !emp.Activo)
            {
                return null;
            }

            var rutas = (await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false))
                .Where(r => r.Activa && r.EmpresaId == id)
                .ToList();
            var destinos = (await _store.ListDestinosAsync(cancellationToken).ConfigureAwait(false))
                .Where(d => d.Activo && d.RutasIds.Any(rid => rutas.Any(r => r.Id == rid)))
                .Select(d => new DestinoDto { Id = d.Id, Nombre = d.Nombre })
                .ToList();

            return new EmpresaDetailDto
            {
                Id = emp.Id.ToString(),
                Nombre = emp.Nombre,
                Siglas = emp.Siglas,
                Descripcion = emp.Descripcion,
                TelefonoContacto = emp.TelefonoContacto ?? string.Empty,
                SitioWeb = emp.SitioWeb,
                HorarioAtencion = "05:00 - 22:30",
                CalificacionPromedio = emp.CalificacionPromedio,
                TotalEvaluaciones = emp.TotalEvaluaciones,
                MediosTransporte = emp.MediosTransporte,
                Rutas = rutas.Select(r => new RutaDto
                {
                    Id = r.Id.ToString(),
                    Nombre = r.Nombre,
                    MedioTransporteId = r.MedioTransporteId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Sentido = $"{r.ParadaOrigen} -> {r.ParadaDestino}",
                    PrecioBase = r.TarifaBaseMxn,
                }).ToList(),
                DestinosAsociados = destinos,
                FechaUltimaActualizacion = emp.FechaActualizacion,
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<EmpresaRutaItemDto>> GetRutasAsync(
        Guid id,
        int? medioTransporteId,
        string? paradaId,
        string? diaOperacion,
        CancellationToken cancellationToken)
    {
        var rutas = (await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false))
            .Where(r => r.Activa && r.EmpresaId == id);

        if (medioTransporteId.HasValue)
        {
            rutas = rutas.Where(r => r.MedioTransporteId == medioTransporteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paradaId))
        {
            var term = paradaId.Trim();
            rutas = rutas.Where(r => r.Paradas.Any(p =>
                p.ParadaId.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(diaOperacion))
        {
            var dia = diaOperacion.Trim();
            rutas = rutas.Where(r => r.DiasOperacion.Contains(dia, StringComparison.OrdinalIgnoreCase));
        }

        return rutas.Select(r => new EmpresaRutaItemDto(
            r.Id,
            r.Codigo,
            r.Nombre,
            r.MedioTransporteNombre,
            r.ParadaOrigen,
            r.ParadaDestino,
            r.TarifaBaseMxn,
            r.PrimerHorario,
            r.UltimoHorario,
            r.FrecuenciaMinutos)).ToList();
    }

    public async Task<EmpresaCompararResponseDto> CompararAsync(
        string? paradaOrigenId,
        string? paradaDestinoId,
        string? destinoTuristicoId,
        string? empresasIds,
        CancellationToken cancellationToken)
    {
        var allEmpresas = (await _store.ListEmpresasAsync(cancellationToken).ConfigureAwait(false))
            .Where(e => e.Activo).ToList();
        var allRutas = (await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false))
            .Where(r => r.Activa).ToList();

        HashSet<Guid>? explicitIds = null;
        if (!string.IsNullOrWhiteSpace(empresasIds))
        {
            explicitIds = empresasIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .Take(4)
                .ToHashSet();
        }

        IEnumerable<RutaDoc> filteredRutas = allRutas;
        if (!string.IsNullOrWhiteSpace(paradaOrigenId))
        {
            filteredRutas = filteredRutas.Where(r =>
                r.ParadaOrigen.Contains(paradaOrigenId.Trim(), StringComparison.OrdinalIgnoreCase)
                || r.Paradas.Any(p => string.Equals(p.ParadaId, paradaOrigenId.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(paradaDestinoId))
        {
            filteredRutas = filteredRutas.Where(r =>
                r.ParadaDestino.Contains(paradaDestinoId.Trim(), StringComparison.OrdinalIgnoreCase)
                || r.Paradas.Any(p => string.Equals(p.ParadaId, paradaDestinoId.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(destinoTuristicoId))
        {
            var dest = await _store.GetDestinoByIdAsync(destinoTuristicoId.Trim(), cancellationToken).ConfigureAwait(false);
            if (dest is not null)
            {
                var dRutas = dest.RutasIds.ToHashSet();
                filteredRutas = filteredRutas.Where(r => dRutas.Contains(r.Id));
            }
        }

        var rutasList = filteredRutas.ToList();
        var candidateEmpresas = explicitIds is { Count: > 0 }
            ? allEmpresas.Where(e => explicitIds.Contains(e.Id))
            : allEmpresas.Where(e => rutasList.Any(r => r.EmpresaId == e.Id));

        var comparadas = candidateEmpresas
            .Take(4)
            .Select(e =>
            {
                var empRutas = allRutas.Where(r => r.EmpresaId == e.Id).ToList();
                var tarifaProm = empRutas.Count == 0 ? 0m : Math.Round(empRutas.Average(r => r.TarifaBaseMxn), 2);
                var freqMin = empRutas.Count == 0 ? 0 : (int)Math.Round(empRutas.Average(r => r.FrecuenciaMinutos));
                return new EmpresaComparadaItemDto(
                    e.Id,
                    e.Nombre,
                    (decimal)e.CalificacionPromedio,
                    e.TotalEvaluaciones,
                    e.MediosTransporte,
                    tarifaProm,
                    freqMin,
                    empRutas.Select(r => r.PrimerHorario).OrderBy(x => x).FirstOrDefault(),
                    empRutas.Select(r => r.UltimoHorario).OrderByDescending(x => x).FirstOrDefault(),
                    empRutas.Select(r => r.Nombre).ToList());
            })
            .ToList();

        var trayecto = string.IsNullOrWhiteSpace(paradaOrigenId) && string.IsNullOrWhiteSpace(paradaDestinoId)
            ? "Comparativa general"
            : $"{paradaOrigenId ?? "*"} -> {paradaDestinoId ?? "*"}";

        return new EmpresaCompararResponseDto(trayecto, comparadas);
    }

    public async Task<EmpresaResponseDto> CreateAsync(EmpresaCreateDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.nombre))
        {
            throw new BadHttpRequestException("El nombre de la empresa es obligatorio.");
        }

        var existing = await _store.ListEmpresasAsync(cancellationToken).ConfigureAwait(false);
        if (existing.Any(e => string.Equals(e.Nombre, dto.nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException("Ya existe una empresa con ese nombre.");
        }

        var doc = new EmpresaDoc
        {
            Id = Guid.NewGuid(),
            Nombre = dto.nombre.Trim(),
            Siglas = dto.siglas?.Trim(),
            Descripcion = dto.descripcion?.Trim(),
            MunicipioBase = string.IsNullOrWhiteSpace(dto.municipioBase) ? "Tula de Allende" : dto.municipioBase.Trim(),
            TelefonoContacto = dto.telefonoContacto?.Trim(),
            SitioWeb = dto.sitioWeb?.Trim(),
            LogoUrl = dto.logoUrl?.Trim(),
            FuenteDatoId = dto.fuenteDatoId,
            CalificacionPromedio = 0,
            TotalEvaluaciones = 0,
            Activo = true,
            FechaActualizacion = DateTime.UtcNow,
        };

        await _store.UpsertEmpresaAsync(doc, cancellationToken).ConfigureAwait(false);
        await DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        return ToResponseDto(doc);
    }

    public async Task<EmpresaResponseDto> UpdateAsync(Guid id, EmpresaUpdateDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var doc = await _store.GetEmpresaByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Empresa {id} no encontrada.");

        if (!string.IsNullOrWhiteSpace(dto.nombre))
        {
            doc.Nombre = dto.nombre.Trim();
        }

        doc.Siglas = dto.siglas?.Trim() ?? doc.Siglas;
        doc.Descripcion = dto.descripcion?.Trim() ?? doc.Descripcion;
        doc.MunicipioBase = string.IsNullOrWhiteSpace(dto.municipioBase) ? doc.MunicipioBase : dto.municipioBase.Trim();
        doc.TelefonoContacto = dto.telefonoContacto?.Trim() ?? doc.TelefonoContacto;
        doc.SitioWeb = dto.sitioWeb?.Trim() ?? doc.SitioWeb;
        doc.LogoUrl = dto.logoUrl?.Trim() ?? doc.LogoUrl;
        doc.FuenteDatoId = dto.fuenteDatoId ?? doc.FuenteDatoId;
        doc.Activo = dto.activo;
        doc.FechaActualizacion = DateTime.UtcNow;

        await _store.UpsertEmpresaAsync(doc, cancellationToken).ConfigureAwait(false);
        await DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        return ToResponseDto(doc);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var doc = await _store.GetEmpresaByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Empresa {id} no encontrada.");
        doc.Activo = false;
        doc.FechaActualizacion = DateTime.UtcNow;
        await _store.UpsertEmpresaAsync(doc, cancellationToken).ConfigureAwait(false);
        await DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
    }

    private static EmpresaResponseDto ToResponseDto(EmpresaDoc e) =>
        new(
            e.Id,
            e.Nombre,
            e.Siglas,
            e.Descripcion,
            e.MunicipioBase,
            e.TelefonoContacto,
            e.SitioWeb,
            e.LogoUrl,
            (decimal)e.CalificacionPromedio,
            e.TotalEvaluaciones,
            e.Activo,
            e.FechaActualizacion);
}
