using System.Globalization;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Transport;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Servicio unificado NoSQL + Redis para Rutas, Paradas, Mapa, Horarios, Tarifas y Destinos (EP3, EP4, EP5, EP-AD-01).
/// </summary>
public interface ITransportCatalogService
{
    Task<PagedRutasResponse> ListRutasAsync(string? empresa, string? destino, int pagina, int tamanoPagina, CancellationToken ct);
    Task<IReadOnlyList<RutaListItemDto>> SearchRutasAsync(string? origen, string? destino, string? municipio, string? medioTransporte, decimal? tarifaMax, string? ordenarPor, CancellationToken ct);
    Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteRutasAsync(string q, int limite, CancellationToken ct);
    Task<RutaListItemDto?> GetRutaByIdAsync(Guid id, CancellationToken ct);
    Task<RutaParadasResponse?> GetRutaParadasAsync(Guid id, CancellationToken ct);
    Task<RutaGeometriaResponse?> GetRutaGeometriaAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<ParadaDetallePublicoDto>> ListParadasAsync(string? municipio, CancellationToken ct);
    Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteParadasAsync(string q, int limite, CancellationToken ct);
    Task<ParadaDetallePublicoDto?> GetParadaByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<ParadaCercanaDto>> GetParadasCercanasAsync(double lat, double lon, double radioKm, CancellationToken ct);
    Task<MapaElementosResponse> GetElementosMapaAsync(double? lat, double? lon, double radioKm, string? tipo, CancellationToken ct);

    Task<IReadOnlyList<HorarioItemDto>> ListHorariosAsync(Guid? rutaId, string? diaSemana, CancellationToken ct);
    Task<ProximaSalidaResponse?> GetProximaSalidaAsync(Guid rutaId, string? horaActual, CancellationToken ct);
    Task<IReadOnlyList<TarifaItemDto>> ListTarifasAsync(Guid? rutaId, bool? soloVigentes, CancellationToken ct);
    Task<CalculoTarifaResponse?> CalcularTarifaAsync(Guid rutaId, string? tipoPasajero, CancellationToken ct);

    Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteDestinosAsync(string q, int limite, CancellationToken ct);
    Task<IReadOnlyList<DestinoTuristicoDto>> ListDestinosAsync(string? categoria, string? municipio, string? q, CancellationToken ct);
    Task<IReadOnlyList<DestinoCategoriaCountDto>> GetCategoriasDestinosAsync(CancellationToken ct);
    Task<DestinoTuristicoDto?> GetDestinoByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<RutaListItemDto>?> GetRutasDeDestinoAsync(string id, CancellationToken ct);

    Task<EstadoActualizacionCatalogosResponse> GetEstadoActualizacionAsync(CancellationToken ct);
    Task InvalidateCatalogCacheAsync(CancellationToken ct);
}

internal sealed class TransportCatalogService : ITransportCatalogService
{
    private const string CachePrefix = "catalog:nosql:";
    private const string CacheVersionKey = CachePrefix + "version";

    private readonly IBusTrackNoSqlStore _store;
    private readonly IDistributedCache _cache;
    private readonly TimeProvider _time;

    public TransportCatalogService(
        IBusTrackNoSqlStore store,
        IDistributedCache cache,
        TimeProvider time)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public async Task<PagedRutasResponse> ListRutasAsync(
        string? empresa,
        string? destino,
        int pagina,
        int tamanoPagina,
        CancellationToken ct)
    {
        var p = Math.Max(1, pagina);
        var s = Math.Clamp(tamanoPagina, 1, 100);
        var version = await DistributedCacheHelper.GetVersionAsync(_cache, CacheVersionKey, ct).ConfigureAwait(false);
        var cacheKey = $"{CachePrefix}v{version}:rutas:{empresa}:{destino}:{p}:{s}";

        return await DistributedCacheHelper.GetOrSetAsync(_cache, cacheKey, TimeSpan.FromSeconds(60), async () =>
        {
            var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa);
            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var emp = empresa.Trim();
                rutas = rutas.Where(r => r.EmpresaNombre.Contains(emp, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(destino))
            {
                var dest = destino.Trim();
                rutas = rutas.Where(r => r.ParadaDestino.Contains(dest, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = rutas.Select(MapRuta).ToList();
            return new PagedRutasResponse(
                filtered.Count,
                p,
                s,
                filtered.Skip((p - 1) * s).Take(s).ToList());
        }, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RutaListItemDto>> SearchRutasAsync(
        string? origen,
        string? destino,
        string? municipio,
        string? medioTransporte,
        decimal? tarifaMax,
        string? ordenarPor,
        CancellationToken ct)
    {
        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa);
        if (!string.IsNullOrWhiteSpace(origen))
        {
            var o = origen.Trim();
            rutas = rutas.Where(r =>
                r.ParadaOrigen.Contains(o, StringComparison.OrdinalIgnoreCase)
                || r.Nombre.Contains(o, StringComparison.OrdinalIgnoreCase)
                || r.Paradas.Any(p => p.Nombre.Contains(o, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(destino))
        {
            var d = destino.Trim();
            rutas = rutas.Where(r =>
                r.ParadaDestino.Contains(d, StringComparison.OrdinalIgnoreCase)
                || r.Nombre.Contains(d, StringComparison.OrdinalIgnoreCase)
                || r.Paradas.Any(p => p.Nombre.Contains(d, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var m = municipio.Trim();
            rutas = rutas.Where(r => r.Municipio.Contains(m, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(medioTransporte))
        {
            var mt = medioTransporte.Trim();
            rutas = rutas.Where(r => r.MedioTransporteNombre.Contains(mt, StringComparison.OrdinalIgnoreCase));
        }

        if (tarifaMax.HasValue)
        {
            rutas = rutas.Where(r => r.TarifaBaseMxn <= tarifaMax.Value);
        }

        rutas = (ordenarPor ?? "tarifa").ToLowerInvariant() switch
        {
            "tiempo" => rutas.OrderBy(r => r.DuracionEstimadaMin),
            "calificacion" => rutas.OrderByDescending(r => r.CalificacionPromedio),
            _ => rutas.OrderBy(r => r.TarifaBaseMxn),
        };

        return rutas.Select(MapRuta).ToList();
    }

    public async Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteRutasAsync(string q, int limite, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return [];
        }

        var term = q.Trim();
        var rutas = await _store.ListRutasAsync(ct).ConfigureAwait(false);
        return rutas
            .Where(r => r.Activa && (r.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.ParadaOrigen.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.ParadaDestino.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Take(Math.Clamp(limite, 1, 20))
            .Select(r => new AutocompleteItemDto(r.Id.ToString(), r.Nombre, $"{r.Codigo} · {r.EmpresaNombre}"))
            .ToList();
    }

    public async Task<RutaListItemDto?> GetRutaByIdAsync(Guid id, CancellationToken ct)
    {
        var r = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false);
        return r is null || !r.Activa ? null : MapRuta(r);
    }

    public async Task<RutaParadasResponse?> GetRutaParadasAsync(Guid id, CancellationToken ct)
    {
        var r = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false);
        if (r is null || !r.Activa)
        {
            return null;
        }

        var stops = r.Paradas
            .OrderBy(p => p.OrdenSecuencia)
            .Select(p => new RutaParadaItemDto(p.ParadaId, p.Nombre, p.Latitud, p.Longitud, p.OrdenSecuencia))
            .ToList();

        return new RutaParadasResponse(r.Id, r.Nombre, stops.Count, stops);
    }

    public async Task<RutaGeometriaResponse?> GetRutaGeometriaAsync(Guid id, CancellationToken ct)
    {
        var r = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false);
        if (r is null || !r.Activa)
        {
            return null;
        }

        var coords = r.Paradas
            .OrderBy(p => p.OrdenSecuencia)
            .Select(p => new[] { p.Longitud, p.Latitud })
            .ToList();

        return new RutaGeometriaResponse(r.Id, "LineString", coords);
    }

    public async Task<IReadOnlyList<ParadaDetallePublicoDto>> ListParadasAsync(string? municipio, CancellationToken ct)
    {
        var version = await DistributedCacheHelper.GetVersionAsync(_cache, CacheVersionKey, ct).ConfigureAwait(false);
        var key = $"{CachePrefix}v{version}:paradas:{municipio}";
        return await DistributedCacheHelper.GetOrSetAsync(_cache, key, TimeSpan.FromSeconds(120), async () =>
        {
            var paradas = (await _store.ListParadasAsync(ct).ConfigureAwait(false)).Where(p => p.Activo);
            if (!string.IsNullOrWhiteSpace(municipio))
            {
                var mun = municipio.Trim();
                paradas = paradas.Where(p => p.Municipio.Contains(mun, StringComparison.OrdinalIgnoreCase));
            }

            return (IReadOnlyList<ParadaDetallePublicoDto>)paradas.Select(MapParada).ToList();
        }, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteParadasAsync(string q, int limite, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return [];
        }

        var term = q.Trim();
        var paradas = await _store.ListParadasAsync(ct).ConfigureAwait(false);
        return paradas
            .Where(p => p.Activo && (p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Municipio.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Take(Math.Clamp(limite, 1, 20))
            .Select(p => new AutocompleteItemDto(p.Id, p.Nombre, p.Municipio))
            .ToList();
    }

    public async Task<ParadaDetallePublicoDto?> GetParadaByIdAsync(string id, CancellationToken ct)
    {
        var p = await _store.GetParadaByIdAsync(id, ct).ConfigureAwait(false);
        return p is null || !p.Activo ? null : MapParada(p);
    }

    public async Task<IReadOnlyList<ParadaCercanaDto>> GetParadasCercanasAsync(
        double lat,
        double lon,
        double radioKm,
        CancellationToken ct)
    {
        var radius = Math.Clamp(radioKm, 0.1, 100.0);
        var hits = await _store.FindParadasNearAsync(lat, lon, radius, ct).ConfigureAwait(false);
        return hits.Select(x => new ParadaCercanaDto(
            x.Parada.Id,
            x.Parada.Nombre,
            x.Parada.Municipio,
            x.Parada.Ubicacion.Latitud,
            x.Parada.Ubicacion.Longitud,
            x.Parada.RutasDisponibles,
            Math.Round(x.DistanciaKm, 2))).ToList();
    }

    public async Task<MapaElementosResponse> GetElementosMapaAsync(
        double? lat,
        double? lon,
        double radioKm,
        string? tipo,
        CancellationToken ct)
    {
        var items = new List<MapaElementoDto>();
        var incluirParadas = string.IsNullOrWhiteSpace(tipo)
            || string.Equals(tipo, "parada", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipo, "todos", StringComparison.OrdinalIgnoreCase);
        var incluirDestinos = string.IsNullOrWhiteSpace(tipo)
            || string.Equals(tipo, "destino", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipo, "todos", StringComparison.OrdinalIgnoreCase);

        if (incluirParadas)
        {
            var paradas = (await _store.ListParadasAsync(ct).ConfigureAwait(false)).Where(p => p.Activo);
            foreach (var p in paradas)
            {
                if (lat.HasValue && lon.HasValue
                    && GeoMath.DistanceKm(lat.Value, lon.Value, p.Ubicacion.Latitud, p.Ubicacion.Longitud) > radioKm)
                {
                    continue;
                }

                items.Add(new MapaElementoDto(p.Id, "parada", p.Nombre, p.Municipio, p.Ubicacion.Latitud, p.Ubicacion.Longitud, p.Tipo));
            }
        }

        if (incluirDestinos)
        {
            var destinos = (await _store.ListDestinosAsync(ct).ConfigureAwait(false)).Where(d => d.Activo);
            foreach (var d in destinos)
            {
                if (lat.HasValue && lon.HasValue
                    && GeoMath.DistanceKm(lat.Value, lon.Value, d.Ubicacion.Latitud, d.Ubicacion.Longitud) > radioKm)
                {
                    continue;
                }

                items.Add(new MapaElementoDto(d.Id, "destino", d.Nombre, d.Municipio, d.Ubicacion.Latitud, d.Ubicacion.Longitud, d.Categoria));
            }
        }

        return new MapaElementosResponse(items.Count, items);
    }

    public async Task<IReadOnlyList<HorarioItemDto>> ListHorariosAsync(
        Guid? rutaId,
        string? diaSemana,
        CancellationToken ct)
    {
        var list = (await _store.ListHorariosAsync(rutaId, ct).ConfigureAwait(false)).Where(h => h.Activo);
        if (!string.IsNullOrWhiteSpace(diaSemana))
        {
            var dia = diaSemana.Trim();
            list = list.Where(h => h.DiaSemana.Contains(dia, StringComparison.OrdinalIgnoreCase)
                || string.Equals(h.DiaSemana, "Todos", StringComparison.OrdinalIgnoreCase));
        }

        return list.Select(h => new HorarioItemDto(
            h.Id,
            h.RutaId,
            h.DiaSemana,
            h.HoraInicio,
            h.HoraFin,
            h.FrecuenciaMin,
            h.SalidasFijas,
            h.Activo)).ToList();
    }

    public async Task<ProximaSalidaResponse?> GetProximaSalidaAsync(Guid rutaId, string? horaActual, CancellationToken ct)
    {
        var horarios = await _store.ListHorariosAsync(rutaId, ct).ConfigureAwait(false);
        var horario = horarios.FirstOrDefault(h => h.Activo);
        if (horario is null)
        {
            return null;
        }

        var localNow = TimeOnly.FromDateTime(_time.GetUtcNow().UtcDateTime.AddHours(-6));
        if (!string.IsNullOrWhiteSpace(horaActual)
            && TimeOnly.TryParseExact(horaActual.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            localNow = parsed;
        }

        var salidas = horario.SalidasFijas
            .Select(s => TimeOnly.TryParseExact(s, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? (TimeOnly?)t : null)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .OrderBy(t => t)
            .ToList();

        var next = salidas.FirstOrDefault(t => t >= localNow);
        if (next == default && salidas.Count > 0)
        {
            next = salidas[0];
        }

        var diffMin = next >= localNow
            ? (int)(next - localNow).TotalMinutes
            : (int)((TimeSpan.FromHours(24) - localNow.ToTimeSpan()) + next.ToTimeSpan()).TotalMinutes;

        return new ProximaSalidaResponse(
            rutaId,
            localNow.ToString("HH:mm", CultureInfo.InvariantCulture),
            next == default ? null : next.ToString("HH:mm", CultureInfo.InvariantCulture),
            Math.Max(0, diffMin),
            horario.FrecuenciaMin);
    }

    public async Task<IReadOnlyList<TarifaItemDto>> ListTarifasAsync(Guid? rutaId, bool? soloVigentes, CancellationToken ct)
    {
        var list = await _store.ListTarifasAsync(rutaId, ct).ConfigureAwait(false);
        if (soloVigentes != false)
        {
            list = list.Where(t => t.Vigente).ToList();
        }

        return list.Select(t => new TarifaItemDto(
            t.Id,
            t.RutaId,
            t.TipoTarifa,
            t.MontoMxn,
            t.DescuentoEstudiantePct,
            t.Moneda,
            t.Vigente,
            t.Notas)).ToList();
    }

    public async Task<CalculoTarifaResponse?> CalcularTarifaAsync(Guid rutaId, string? tipoPasajero, CancellationToken ct)
    {
        var tarifas = await _store.ListTarifasAsync(rutaId, ct).ConfigureAwait(false);
        var tarifa = tarifas.FirstOrDefault(t => t.Vigente);
        if (tarifa is null)
        {
            var ruta = await _store.GetRutaByIdAsync(rutaId, ct).ConfigureAwait(false);
            if (ruta is null)
            {
                return null;
            }

            tarifa = new TarifaDoc
            {
                Id = $"TAR-AUTO-{ruta.Codigo}",
                RutaId = ruta.Id,
                MontoMxn = ruta.TarifaBaseMxn,
                DescuentoEstudiantePct = 25,
                Moneda = "MXN",
                Vigente = true,
            };
        }

        var pasajeroNorm = string.IsNullOrWhiteSpace(tipoPasajero) ? "General" : tipoPasajero.Trim();
        var discountPct = 0;
        if (string.Equals(pasajeroNorm, "Estudiante", StringComparison.OrdinalIgnoreCase))
        {
            discountPct = tarifa.DescuentoEstudiantePct;
        }
        else if (string.Equals(pasajeroNorm, "AdultoMayor", StringComparison.OrdinalIgnoreCase))
        {
            discountPct = 50;
        }

        var finalAmount = Math.Round(tarifa.MontoMxn * (1m - (discountPct / 100m)), 2);
        return new CalculoTarifaResponse(rutaId, pasajeroNorm, tarifa.MontoMxn, finalAmount, discountPct, tarifa.Moneda);
    }

    public async Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteDestinosAsync(string q, int limite, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return [];
        }

        var term = q.Trim();
        var list = await _store.ListDestinosAsync(ct).ConfigureAwait(false);
        return list
            .Where(d => d.Activo && (d.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                || d.Municipio.Contains(term, StringComparison.OrdinalIgnoreCase)
                || d.Categoria.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Take(Math.Clamp(limite, 1, 20))
            .Select(d => new AutocompleteItemDto(d.Id, d.Nombre, $"{d.Categoria} · {d.Municipio}"))
            .ToList();
    }

    public async Task<IReadOnlyList<DestinoTuristicoDto>> ListDestinosAsync(
        string? categoria,
        string? municipio,
        string? q,
        CancellationToken ct)
    {
        var list = (await _store.ListDestinosAsync(ct).ConfigureAwait(false)).Where(d => d.Activo);
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var c = categoria.Trim();
            list = list.Where(d => string.Equals(d.Categoria, c, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var m = municipio.Trim();
            list = list.Where(d => d.Municipio.Contains(m, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            list = list.Where(d =>
                d.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                || d.Descripcion.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return list.Select(MapDestino).ToList();
    }

    public async Task<IReadOnlyList<DestinoCategoriaCountDto>> GetCategoriasDestinosAsync(CancellationToken ct)
    {
        var list = (await _store.ListDestinosAsync(ct).ConfigureAwait(false)).Where(d => d.Activo);
        return list
            .GroupBy(d => d.Categoria, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DestinoCategoriaCountDto(g.Key, g.Count()))
            .OrderByDescending(c => c.Total)
            .ToList();
    }

    public async Task<DestinoTuristicoDto?> GetDestinoByIdAsync(string id, CancellationToken ct)
    {
        var d = await _store.GetDestinoByIdAsync(id, ct).ConfigureAwait(false);
        return d is null || !d.Activo ? null : MapDestino(d);
    }

    public async Task<IReadOnlyList<RutaListItemDto>?> GetRutasDeDestinoAsync(string id, CancellationToken ct)
    {
        var d = await _store.GetDestinoByIdAsync(id, ct).ConfigureAwait(false);
        if (d is null || !d.Activo)
        {
            return null;
        }

        var ids = d.RutasIds.ToHashSet();
        var rutas = await _store.ListRutasAsync(ct).ConfigureAwait(false);
        return rutas.Where(r => r.Activa && ids.Contains(r.Id)).Select(MapRuta).ToList();
    }

    public async Task<EstadoActualizacionCatalogosResponse> GetEstadoActualizacionAsync(CancellationToken ct)
    {
        var empresas = await _store.ListEmpresasAsync(ct).ConfigureAwait(false);
        var rutas = await _store.ListRutasAsync(ct).ConfigureAwait(false);
        var paradas = await _store.ListParadasAsync(ct).ConfigureAwait(false);
        var destinos = await _store.ListDestinosAsync(ct).ConfigureAwait(false);
        var updated = _store.CatalogUpdatedAtUtc;
        var version = $"1.{updated.Ticks % 10000}.{rutas.Count + paradas.Count}";

        return new EstadoActualizacionCatalogosResponse(
            version,
            updated,
            empresas.Count(e => e.Activo),
            rutas.Count(r => r.Activa),
            paradas.Count(p => p.Activo),
            destinos.Count(d => d.Activo));
    }

    public Task InvalidateCatalogCacheAsync(CancellationToken ct) =>
        DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, ct);

    private static RutaListItemDto MapRuta(RutaDoc r) =>
        new(
            r.Id,
            r.Codigo,
            r.Nombre,
            r.EmpresaNombre,
            r.MedioTransporteNombre,
            r.ParadaOrigen,
            r.ParadaDestino,
            r.TarifaBaseMxn,
            r.PrimerHorario,
            r.UltimoHorario,
            r.FrecuenciaMinutos,
            r.DiasOperacion,
            r.Activa,
            r.CalificacionPromedio,
            r.DuracionEstimadaMin);

    private static ParadaDetallePublicoDto MapParada(ParadaDoc p) =>
        new(
            p.Id,
            p.Nombre,
            p.Municipio,
            p.Ubicacion.Latitud,
            p.Ubicacion.Longitud,
            p.Techada,
            p.RutasDisponibles,
            p.Referencias);

    private static DestinoTuristicoDto MapDestino(DestinoDoc d) =>
        new(
            d.Id,
            d.Nombre,
            d.Categoria,
            d.Municipio,
            d.Descripcion,
            d.ParadaCercanaId,
            d.Ubicacion.Latitud,
            d.Ubicacion.Longitud,
            d.CalificacionPromedio,
            d.RutasIds,
            d.Activo);
}
