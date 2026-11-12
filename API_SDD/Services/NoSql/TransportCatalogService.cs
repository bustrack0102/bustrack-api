using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Operational;
using ApiSDD.Dtos.Transport;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Servicio unificado NoSQL + Redis para Rutas, Paradas, Mapa, Horarios, Tarifas y Destinos (EP3, EP4, EP5, EP-AD-01).
/// Contratos EP-RP-01..16 alineados al Excel BusTrack_Componentes_Backend_Movil_Azure (módulo 3).
/// </summary>
public interface ITransportCatalogService
{
    Task<PagedRutasCatalogoResponse> ListRutasAsync(
        Guid? empresaId,
        int? medioTransporteId,
        string? municipioOrigen,
        string? municipioDestino,
        bool? activa,
        string? empresa,
        string? destino,
        int pagina,
        int tamanoPagina,
        CancellationToken ct);

    Task<RutaBusquedaResponse> SearchRutasAsync(
        Guid? paradaOrigenId,
        Guid? paradaDestinoId,
        string? destinoTuristicoId,
        Guid? empresaId,
        int? medioTransporteId,
        decimal? tarifaMaxima,
        string? diaOperacion,
        string? horaSalida,
        string? ordenarPor,
        string? origen,
        string? destino,
        CancellationToken ct);

    Task<IReadOnlyList<RutaAutocompleteItemDto>> AutocompleteRutasAsync(
        string q,
        Guid? empresaId,
        int limite,
        CancellationToken ct);

    Task<RutaDetalleResponse?> GetRutaByIdAsync(Guid id, CancellationToken ct);

    Task<RutaParadasResponse?> GetRutaParadasAsync(
        Guid id,
        bool? soloAscenso,
        bool? soloDescenso,
        CancellationToken ct);

    Task<RutaGeometriaDetalleDto?> GetRutaGeometriaAsync(
        Guid id,
        string? paradaSubidaId,
        string? paradaBajadaId,
        CancellationToken ct);

    Task<PagedParadasResponse> ListParadasAsync(
        string? municipio,
        Guid? rutaId,
        bool? esTerminal,
        double? lat,
        double? lng,
        double radioMetros,
        int pagina,
        int tamanoPagina,
        CancellationToken ct);

    Task<IReadOnlyList<ParadaAutocompleteItemDto>> AutocompleteParadasAsync(
        string q,
        string? municipio,
        bool? soloOrigen,
        Guid? rutaId,
        int limite,
        CancellationToken ct);

    Task<ParadaDetalleDto?> GetParadaByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<ParadaCercanaDto>> GetParadasCercanasAsync(double lat, double lon, double radioKm, CancellationToken ct);

    Task<MapaElementosDetalleResponse> GetElementosMapaAsync(
        double? minLat,
        double? minLng,
        double? maxLat,
        double? maxLng,
        bool? incluirParadas,
        bool? incluirDestinos,
        string? categoriaDestino,
        double? lat,
        double? lon,
        double radioKm,
        string? tipo,
        CancellationToken ct);

    Task<IReadOnlyList<HorarioItemDto>> ListHorariosAsync(Guid? rutaId, string? diaSemana, CancellationToken ct);
    Task<ProximaSalidaResponse?> GetProximaSalidaAsync(Guid rutaId, string? horaActual, CancellationToken ct);
    Task<IReadOnlyList<TarifaItemDto>> ListTarifasAsync(Guid? rutaId, bool? soloVigentes, CancellationToken ct);
    Task<CalculoTarifaResponse?> CalcularTarifaAsync(Guid rutaId, string? tipoPasajero, CancellationToken ct);

    Task<IReadOnlyList<AutocompleteItemDto>> AutocompleteDestinosAsync(string q, int limite, CancellationToken ct);
    Task<IReadOnlyList<DestinoTuristicoDto>> ListDestinosAsync(string? categoria, string? municipio, string? q, CancellationToken ct);
    Task<IReadOnlyList<DestinoCategoriaCountDto>> GetCategoriasDestinosAsync(CancellationToken ct);
    Task<DestinoTuristicoDto?> GetDestinoByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<RutaListItemDto>?> GetRutasDeDestinoAsync(string id, CancellationToken ct);

    Task<ParadaDetalleDto> CreateParadaAsync(ParadaUpsertRequest request, CancellationToken ct);
    Task<ParadaDetalleDto> UpdateParadaAsync(string id, ParadaUpsertRequest request, CancellationToken ct);
    Task DeactivateParadaAsync(string id, CancellationToken ct);

    Task<RutaDetalleResponse> CreateRutaAsync(RutaUpsertRequest request, CancellationToken ct);
    Task<RutaDetalleResponse> UpdateRutaAsync(Guid id, RutaUpsertRequest request, CancellationToken ct);
    Task DeactivateRutaAsync(Guid id, CancellationToken ct);

    Task<HorarioItemDto> CreateHorarioAsync(HorarioUpsertRequest request, CancellationToken ct);
    Task<HorarioItemDto> UpdateHorarioAsync(string id, HorarioUpsertRequest request, CancellationToken ct);
    Task DeactivateHorarioAsync(string id, CancellationToken ct);

    Task<TarifaItemDto> CreateTarifaAsync(TarifaUpsertRequest request, CancellationToken ct);
    Task<TarifaItemDto> UpdateTarifaAsync(string id, TarifaUpsertRequest request, CancellationToken ct);
    Task DeactivateTarifaAsync(string id, CancellationToken ct);

    Task<DestinoTuristicoDto> CreateDestinoAsync(DestinoUpsertRequest request, CancellationToken ct);
    Task<DestinoTuristicoDto> UpdateDestinoAsync(string id, DestinoUpsertRequest request, CancellationToken ct);
    Task DeactivateDestinoAsync(string id, CancellationToken ct);

    Task<EstadoActualizacionCatalogosResponse> GetEstadoActualizacionAsync(CancellationToken ct);
    Task InvalidateCatalogCacheAsync(CancellationToken ct);
}

internal sealed partial class TransportCatalogService : ITransportCatalogService
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

    // ---------------------------------------------------------------------
    // EP-RP-06: catálogo de rutas
    // ---------------------------------------------------------------------
    public async Task<PagedRutasCatalogoResponse> ListRutasAsync(
        Guid? empresaId,
        int? medioTransporteId,
        string? municipioOrigen,
        string? municipioDestino,
        bool? activa,
        string? empresa,
        string? destino,
        int pagina,
        int tamanoPagina,
        CancellationToken ct)
    {
        var p = Math.Max(1, pagina);
        var s = Math.Clamp(tamanoPagina, 1, 100);
        var filtrarActiva = activa ?? true;

        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false))
            .Where(r => r.Activa == filtrarActiva);

        if (empresaId.HasValue)
        {
            rutas = rutas.Where(r => r.EmpresaId == empresaId.Value);
        }

        if (medioTransporteId.HasValue)
        {
            rutas = rutas.Where(r => r.MedioTransporteId == medioTransporteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(municipioOrigen))
        {
            var m = Normalize(municipioOrigen);
            rutas = rutas.Where(r => Normalize(r.Municipio).Contains(m, StringComparison.Ordinal)
                || Normalize(r.ParadaOrigen).Contains(m, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(municipioDestino))
        {
            var m = Normalize(municipioDestino);
            rutas = rutas.Where(r => Normalize(r.ParadaDestino).Contains(m, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(empresa))
        {
            var e = Normalize(empresa);
            rutas = rutas.Where(r => Normalize(r.EmpresaNombre).Contains(e, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(destino))
        {
            var d = Normalize(destino);
            rutas = rutas.Where(r => Normalize(r.ParadaDestino).Contains(d, StringComparison.Ordinal)
                || Normalize(r.Nombre).Contains(d, StringComparison.Ordinal));
        }

        var filtered = rutas.OrderBy(r => r.Codigo, StringComparer.OrdinalIgnoreCase).ToList();
        var items = filtered.Skip((p - 1) * s).Take(s).Select(MapRutaCatalogo).ToList();
        return new PagedRutasCatalogoResponse(filtered.Count, p, items);
    }

    // ---------------------------------------------------------------------
    // EP-RP-04: búsqueda origen-destino con sentido de recorrido
    // ---------------------------------------------------------------------
    [SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Búsqueda multicriterio con múltiples filtros del contrato EP-RP-04.")]
    public async Task<RutaBusquedaResponse> SearchRutasAsync(
        Guid? paradaOrigenId,
        Guid? paradaDestinoId,
        string? destinoTuristicoId,
        Guid? empresaId,
        int? medioTransporteId,
        decimal? tarifaMaxima,
        string? diaOperacion,
        string? horaSalida,
        string? ordenarPor,
        string? origen,
        string? destino,
        CancellationToken ct)
    {
        var paradas = await _store.ListParadasAsync(ct).ConfigureAwait(false);
        var paradasById = paradas.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa).ToList();
        var horarios = await _store.ListHorariosAsync(null, ct).ConfigureAwait(false);
        var empresas = await _store.ListEmpresasAsync(ct).ConfigureAwait(false);

        paradasById.TryGetValue(paradaOrigenId?.ToString() ?? string.Empty, out var origenParada);
        var origenTexto = origenParada?.Nombre ?? origen;
        if (origenParada is null && paradaOrigenId.HasValue)
        {
            origenTexto = paradaOrigenId.Value.ToString();
        }

        DestinoDoc? destinoDoc = null;
        if (!string.IsNullOrWhiteSpace(destinoTuristicoId))
        {
            destinoDoc = await _store.GetDestinoByIdAsync(destinoTuristicoId.Trim(), ct).ConfigureAwait(false);
        }

        paradasById.TryGetValue(paradaDestinoId?.ToString() ?? string.Empty, out var destinoParada);
        var destinoTexto = destinoParada?.Nombre ?? destinoDoc?.Nombre ?? destino;

        var empresaById = empresas.ToDictionary(e => e.Id);
        var results = new List<RutaBusquedaAlternativaDto>();

        foreach (var ruta in rutas)
        {
            if (empresaId.HasValue && ruta.EmpresaId != empresaId.Value)
            {
                continue;
            }

            if (medioTransporteId.HasValue && ruta.MedioTransporteId != medioTransporteId.Value)
            {
                continue;
            }

            if (tarifaMaxima.HasValue && ruta.TarifaBaseMxn > tarifaMaxima.Value)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(diaOperacion) && !DayMatches(ruta, diaOperacion, horarios))
            {
                continue;
            }

            var stops = ruta.Paradas.OrderBy(x => x.OrdenSecuencia).ToList();
            int? origenIdx = null;
            int? destinoIdx = null;

            if (paradaOrigenId.HasValue || !string.IsNullOrWhiteSpace(origen))
            {
                origenIdx = FindStopIndex(stops, paradaOrigenId?.ToString(), origenTexto ?? origen);
                if (origenIdx is null)
                {
                    continue;
                }
            }

            if (destinoDoc is not null)
            {
                var pertenece = destinoDoc.RutasIds.Contains(ruta.Id);
                var destinoStop = FindStopIndex(stops, destinoDoc.ParadaCercanaId, null)
                    ?? FindStopIndex(stops, null, destinoDoc.Nombre);
                if (!pertenece && destinoStop is null)
                {
                    continue;
                }

                destinoIdx = destinoStop;
            }
            else if (paradaDestinoId.HasValue || !string.IsNullOrWhiteSpace(destino))
            {
                destinoIdx = FindStopIndex(stops, paradaDestinoId?.ToString(), destinoTexto ?? destino);
                if (destinoIdx is null)
                {
                    continue;
                }
            }

            if (origenIdx.HasValue && destinoIdx.HasValue && origenIdx.Value >= destinoIdx.Value)
            {
                continue;
            }

            var tramo = origenIdx.HasValue && destinoIdx.HasValue
                ? destinoIdx.Value - origenIdx.Value + 1
                : stops.Count;

            var tiempo = ComputeTramoTime(ruta, stops, origenIdx, destinoIdx);
            var tarifa = ComputeTramoFare(ruta, stops, origenIdx, destinoIdx);
            var siguiente = ComputeNextDeparture(ruta, horarios, horaSalida);
            empresaById.TryGetValue(ruta.EmpresaId, out var empresa);
            var calificacion = empresa?.CalificacionPromedio ?? ruta.CalificacionPromedio;

            results.Add(new RutaBusquedaAlternativaDto(
                ruta.Id,
                ruta.Codigo,
                ruta.Nombre,
                new RutaBusquedaEmpresaDto(ruta.EmpresaId, ruta.EmpresaNombre, calificacion),
                ruta.MedioTransporteNombre,
                origenIdx.HasValue ? stops[origenIdx.Value].Nombre : ruta.ParadaOrigen,
                destinoIdx.HasValue ? stops[destinoIdx.Value].Nombre : ruta.ParadaDestino,
                tramo,
                tiempo,
                tarifa,
                siguiente,
                ruta.FechaActualizacion ?? _store.CatalogUpdatedAtUtc));
        }

        results = (ordenarPor ?? "tiempo").ToLowerInvariant() switch
        {
            "precio" or "tarifa" => results.OrderBy(r => r.TarifaAproximadaMxn).ToList(),
            "calificacion" => results.OrderByDescending(r => r.Empresa.Calificacion).ToList(),
            _ => results.OrderBy(r => r.TiempoEstimadoMin).ToList(),
        };

        return new RutaBusquedaResponse(
            origenTexto,
            destinoTexto,
            results.Count,
            results.Count == 0 ? "No se encontraron rutas para el origen y destino seleccionados." : null,
            results);
    }

    // ---------------------------------------------------------------------
    // EP-RP-05: autocompletado de rutas
    // ---------------------------------------------------------------------
    public async Task<IReadOnlyList<RutaAutocompleteItemDto>> AutocompleteRutasAsync(
        string q,
        Guid? empresaId,
        int limite,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return [];
        }

        var term = Normalize(q);
        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa);
        if (empresaId.HasValue)
        {
            rutas = rutas.Where(r => r.EmpresaId == empresaId.Value);
        }

        return rutas
            .Where(r => Normalize(r.Nombre).Contains(term, StringComparison.Ordinal)
                || Normalize(r.Codigo).Contains(term, StringComparison.Ordinal)
                || Normalize(r.ParadaOrigen).Contains(term, StringComparison.Ordinal)
                || Normalize(r.ParadaDestino).Contains(term, StringComparison.Ordinal)
                || Normalize(r.EmpresaNombre).Contains(term, StringComparison.Ordinal))
            .OrderByDescending(r => Normalize(r.Codigo).StartsWith(term, StringComparison.Ordinal))
            .ThenBy(r => r.Codigo, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limite, 1, 20))
            .Select(r => new RutaAutocompleteItemDto(
                r.Id,
                r.Codigo,
                r.Nombre,
                r.EmpresaNombre,
                r.MedioTransporteNombre,
                r.ParadaOrigen,
                r.ParadaDestino))
            .ToList();
    }

    // ---------------------------------------------------------------------
    // EP-RP-07: detalle de ruta
    // ---------------------------------------------------------------------
    public async Task<RutaDetalleResponse?> GetRutaByIdAsync(Guid id, CancellationToken ct)
    {
        var ruta = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false);
        if (ruta is null || !ruta.Activa)
        {
            return null;
        }

        var paradas = await _store.ListParadasAsync(ct).ConfigureAwait(false);
        var empresas = await _store.ListEmpresasAsync(ct).ConfigureAwait(false);
        var horarios = await _store.ListHorariosAsync(id, ct).ConfigureAwait(false);
        var tarifas = await _store.ListTarifasAsync(id, ct).ConfigureAwait(false);
        var destinos = await _store.ListDestinosAsync(ct).ConfigureAwait(false);

        var empresa = empresas.FirstOrDefault(e => e.Id == ruta.EmpresaId);
        var paradaOrigen = ResolveRouteParada(ruta, paradas, ruta.ParadaOrigenId, ruta.ParadaOrigen, first: true);
        var paradaDestino = ResolveRouteParada(ruta, paradas, ruta.ParadaDestinoId, ruta.ParadaDestino, first: false);
        var secuencia = BuildStopSequence(ruta, paradas, null, null);
        var destinosCercanos = destinos
            .Where(d => d.Activo && (d.RutasIds.Contains(ruta.Id)
                || string.Equals(d.ParadaCercanaId, ruta.ParadaDestinoId, StringComparison.OrdinalIgnoreCase)))
            .Select(MapDestino)
            .ToList();

        return new RutaDetalleResponse(
            ruta.Id,
            ruta.Codigo,
            ruta.Nombre,
            ruta.Descripcion,
            Math.Round(ruta.DistanciaKm, 2),
            ruta.DuracionEstimadaMin,
            new RutaDetalleEmpresaDto(
                ruta.EmpresaId,
                ruta.EmpresaNombre,
                empresa?.CalificacionPromedio ?? ruta.CalificacionPromedio),
            new RutaDetalleMedioTransporteDto(ruta.MedioTransporteId, ruta.MedioTransporteNombre),
            paradaOrigen is null ? null : new RutaDetalleParadaDto(
                paradaOrigen.Id,
                paradaOrigen.Nombre,
                paradaOrigen.Ubicacion.Latitud,
                paradaOrigen.Ubicacion.Longitud),
            paradaDestino is null ? null : new RutaDetalleParadaDto(
                paradaDestino.Id,
                paradaDestino.Nombre,
                paradaDestino.Ubicacion.Latitud,
                paradaDestino.Ubicacion.Longitud),
            secuencia,
            horarios.Where(h => h.Activo).Select(MapHorario).ToList(),
            tarifas.Where(t => t.Vigente).Select(MapTarifa).ToList(),
            destinosCercanos,
            ruta.FechaActualizacion ?? _store.CatalogUpdatedAtUtc);
    }

    // ---------------------------------------------------------------------
    // EP-RP-08: secuencia ordenada de paradas de una ruta
    // ---------------------------------------------------------------------
    public async Task<RutaParadasResponse?> GetRutaParadasAsync(
        Guid id,
        bool? soloAscenso,
        bool? soloDescenso,
        CancellationToken ct)
    {
        var ruta = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false);
        if (ruta is null || !ruta.Activa)
        {
            return null;
        }

        var paradas = await _store.ListParadasAsync(ct).ConfigureAwait(false);
        var secuencia = BuildStopSequence(ruta, paradas, soloAscenso, soloDescenso);
        return new RutaParadasResponse(ruta.Id, ruta.Nombre, secuencia.Count, secuencia);
    }

    // ---------------------------------------------------------------------
    // EP-RP-09: geometría GeoJSON
    // ---------------------------------------------------------------------
    public async Task<RutaGeometriaDetalleDto?> GetRutaGeometriaAsync(
        Guid id,
        string? paradaSubidaId,
        string? paradaBajadaId,
        CancellationToken ct)
    {
        var ruta = await _store.GetRutaByIdAsync(id, ct).ConfigureAwait(false);
        if (ruta is null || !ruta.Activa)
        {
            return null;
        }

        var paradas = await _store.ListParadasAsync(ct).ConfigureAwait(false);
        var destinos = await _store.ListDestinosAsync(ct).ConfigureAwait(false);
        var stops = ruta.Paradas.OrderBy(p => p.OrdenSecuencia).ToList();

        var coordinates = ruta.Trazado is { Count: > 0 }
            ? ruta.Trazado.ToList()
            : stops.Select(s => new[] { s.Longitud, s.Latitud }).ToList();

        var lngs = coordinates.Select(c => c.Length > 0 ? c[0] : 0).DefaultIfEmpty(0).ToList();
        var lats = coordinates.Select(c => c.Length > 1 ? c[1] : 0).DefaultIfEmpty(0).ToList();
        var boundingBox = new[]
        {
            lngs.Min(),
            lats.Min(),
            lngs.Max(),
            lats.Max(),
        };

        var subidaIdx = FindStopIndex(stops, paradaSubidaId, null);
        var bajadaIdx = FindStopIndex(stops, paradaBajadaId, null);

        var paradasFeatures = stops.Select((s, index) =>
        {
            var parada = FindParada(paradas, s.ParadaId, s.Nombre);
            var resaltada = subidaIdx.HasValue && bajadaIdx.HasValue
                ? index >= subidaIdx.Value && index <= bajadaIdx.Value
                : (bool?)null;
            return new GeoJsonFeatureDto(
                "Feature",
                new GeoJsonPointGeometryDto("Point", [s.Longitud, s.Latitud]),
                new GeoJsonFeaturePropertiesDto(
                    "parada",
                    parada?.Id ?? s.ParadaId,
                    parada?.Nombre ?? s.Nombre,
                    s.OrdenSecuencia,
                    resaltada,
                    null));
        }).ToList();

        var destinosFeatures = destinos
            .Where(d => d.Activo && d.RutasIds.Contains(ruta.Id))
            .Select(d => new GeoJsonFeatureDto(
                "Feature",
                new GeoJsonPointGeometryDto(
                    "Point",
                    [d.Ubicacion.Longitud, d.Ubicacion.Latitud]),
                new GeoJsonFeaturePropertiesDto(
                    "destino",
                    d.Id,
                    d.Nombre,
                    null,
                    null,
                    d.Categoria)))
            .ToList();

        return new RutaGeometriaDetalleDto(
            ruta.Id,
            ruta.Nombre,
            boundingBox,
            new GeoJsonLineStringDto("LineString", coordinates),
            paradasFeatures,
            destinosFeatures);
    }

    // ---------------------------------------------------------------------
    // EP-RP-02: listado y filtro de paradas
    // ---------------------------------------------------------------------
    public async Task<PagedParadasResponse> ListParadasAsync(
        string? municipio,
        Guid? rutaId,
        bool? esTerminal,
        double? lat,
        double? lng,
        double radioMetros,
        int pagina,
        int tamanoPagina,
        CancellationToken ct)
    {
        if (lat.HasValue != lng.HasValue)
        {
            throw new RequestValidationException("Debe enviar latitud y longitud juntas para el filtro espacial.");
        }

        ValidateCoordinates(lat, lng);
        var p = Math.Max(1, pagina);
        var s = Math.Clamp(tamanoPagina, 1, 100);
        var radius = Math.Clamp(radioMetros, 1, 50_000);
        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa).ToList();

        RutaDoc? ruta = null;
        if (rutaId.HasValue)
        {
            ruta = rutas.FirstOrDefault(r => r.Id == rutaId.Value);
            if (ruta is null)
            {
                return new PagedParadasResponse(0, []);
            }
        }

        var paradas = (await _store.ListParadasAsync(ct).ConfigureAwait(false)).Where(x => x.Activo);

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var m = Normalize(municipio);
            paradas = paradas.Where(x => Normalize(x.Municipio).Contains(m, StringComparison.Ordinal));
        }

        if (ruta is not null)
        {
            var stops = ruta.Paradas.ToList();
            paradas = paradas.Where(x => stops.Any(st => StopMatches(st, x)));
        }

        if (esTerminal.HasValue)
        {
            paradas = paradas.Where(x => IsTerminal(x) == esTerminal.Value);
        }

        IEnumerable<(ParadaDoc Parada, double? DistanciaMetros)> projected = lat.HasValue && lng.HasValue
            ? paradas.Select(x => (
                Parada: x,
                DistanciaMetros: (double?)Math.Round(
                    GeoMath.DistanceKm(lat.Value, lng.Value, x.Ubicacion.Latitud, x.Ubicacion.Longitud) * 1000.0,
                    1)))
                .Where(x => x.DistanciaMetros <= radius)
                .OrderBy(x => x.DistanciaMetros)
            : paradas.Select(x => (Parada: x, DistanciaMetros: (double?)null));

        var materialized = projected.ToList();
        var items = materialized
            .Skip((p - 1) * s)
            .Take(s)
            .Select(x => MapParadaListItem(x.Parada, x.DistanciaMetros))
            .ToList();

        return new PagedParadasResponse(materialized.Count, items);
    }

    // ---------------------------------------------------------------------
    // EP-RP-01: autocompletado de paradas
    // ---------------------------------------------------------------------
    public async Task<IReadOnlyList<ParadaAutocompleteItemDto>> AutocompleteParadasAsync(
        string q,
        string? municipio,
        bool? soloOrigen,
        Guid? rutaId,
        int limite,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return [];
        }

        var term = Normalize(q);
        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa).ToList();
        var paradas = (await _store.ListParadasAsync(ct).ConfigureAwait(false)).Where(p => p.Activo).ToList();

        if (rutaId.HasValue)
        {
            var ruta = rutas.FirstOrDefault(r => r.Id == rutaId.Value);
            if (ruta is null)
            {
                return [];
            }

            var stops = ruta.Paradas.ToList();
            paradas = paradas.Where(p => stops.Any(st => StopMatches(st, p))).ToList();
        }

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var m = Normalize(municipio);
            paradas = paradas.Where(p => Normalize(p.Municipio).Contains(m, StringComparison.Ordinal)).ToList();
        }

        if (soloOrigen == true)
        {
            paradas = paradas.Where(p => IsOriginOfAnyRoute(p, rutas, rutaId)).ToList();
        }

        return paradas
            .Where(p => Normalize(p.Nombre).Contains(term, StringComparison.Ordinal)
                || Normalize(p.Referencias ?? string.Empty).Contains(term, StringComparison.Ordinal)
                || Normalize(p.Municipio).Contains(term, StringComparison.Ordinal))
            .OrderByDescending(p => Normalize(p.Nombre).StartsWith(term, StringComparison.Ordinal))
            .ThenBy(p => p.Nombre, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limite, 1, 20))
            .Select(p => new ParadaAutocompleteItemDto(
                p.Id,
                p.Nombre,
                p.Referencias,
                p.Municipio,
                IsTerminal(p),
                p.Ubicacion.Latitud,
                p.Ubicacion.Longitud,
                CountRoutesForParada(p, rutas, rutaId)))
            .ToList();
    }

    // ---------------------------------------------------------------------
    // EP-RP-03: detalle de parada
    // ---------------------------------------------------------------------
    public async Task<ParadaDetalleDto?> GetParadaByIdAsync(string id, CancellationToken ct)
    {
        var parada = await _store.GetParadaByIdAsync(id, ct).ConfigureAwait(false);
        if (parada is null || !parada.Activo)
        {
            return null;
        }

        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false))
            .Where(r => r.Activa && r.Paradas.Any(st => StopMatches(st, parada)))
            .ToList();

        var rutasQuePasan = rutas
            .Select(r =>
            {
                var stop = r.Paradas
                    .Where(st => StopMatches(st, parada))
                    .OrderBy(st => st.OrdenSecuencia)
                    .FirstOrDefault();
                return new RutaQuePasaDto(
                    r.Id,
                    r.Codigo,
                    r.Nombre,
                    r.EmpresaNombre,
                    r.MedioTransporteNombre,
                    stop?.OrdenSecuencia ?? 0);
            })
            .OrderBy(r => r.OrdenEnRuta)
            .ToList();

        return MapParadaDetalle(parada, rutasQuePasan);
    }

    public async Task<IReadOnlyList<ParadaCercanaDto>> GetParadasCercanasAsync(
        double lat,
        double lon,
        double radioKm,
        CancellationToken ct)
    {
        ValidateCoordinates(lat, lon);
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

    // ---------------------------------------------------------------------
    // EP-RP-10: elementos del mapa
    // ---------------------------------------------------------------------
    public async Task<MapaElementosDetalleResponse> GetElementosMapaAsync(
        double? minLat,
        double? minLng,
        double? maxLat,
        double? maxLng,
        bool? incluirParadas,
        bool? incluirDestinos,
        string? categoriaDestino,
        double? lat,
        double? lon,
        double radioKm,
        string? tipo,
        CancellationToken ct)
    {
        var hasAnyBoundingBox = minLat.HasValue || minLng.HasValue || maxLat.HasValue || maxLng.HasValue;
        var hasFullBoundingBox = minLat.HasValue && minLng.HasValue && maxLat.HasValue && maxLng.HasValue;
        if (hasAnyBoundingBox && !hasFullBoundingBox)
        {
            throw new RequestValidationException("El bounding box requiere minLat, minLng, maxLat y maxLng.");
        }

        if (hasFullBoundingBox)
        {
            ValidateCoordinates(minLat, minLng);
            ValidateCoordinates(maxLat, maxLng);
            if (minLat >= maxLat || minLng >= maxLng)
            {
                throw new RequestValidationException("El bounding box debe tener minLat < maxLat y minLng < maxLng.");
            }
        }

        if (lat.HasValue != lon.HasValue)
        {
            throw new RequestValidationException("Debe enviar lat y lng juntas para el filtro radial.");
        }

        ValidateCoordinates(lat, lon);

        var tipoNormalized = string.IsNullOrWhiteSpace(tipo) ? null : tipo.Trim();
        var includeParadas = incluirParadas
            ?? (tipoNormalized is null
                || string.Equals(tipoNormalized, "parada", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipoNormalized, "todos", StringComparison.OrdinalIgnoreCase));
        var includeDestinos = incluirDestinos
            ?? (tipoNormalized is null
                || string.Equals(tipoNormalized, "destino", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipoNormalized, "todos", StringComparison.OrdinalIgnoreCase));

        var paradasResult = new List<MapaParadaItemDto>();
        var destinosResult = new List<MapaDestinoItemDto>();

        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).Where(r => r.Activa).ToList();

        if (includeParadas)
        {
            var paradas = (await _store.ListParadasAsync(ct).ConfigureAwait(false)).Where(p => p.Activo);
            paradas = paradas.Where(p => InsideArea(
                p.Ubicacion.Latitud,
                p.Ubicacion.Longitud,
                minLat,
                minLng,
                maxLat,
                maxLng,
                lat,
                lon,
                radioKm));

            paradasResult = paradas
                .Select(p => new MapaParadaItemDto(
                    p.Id,
                    p.Nombre,
                    p.Ubicacion.Latitud,
                    p.Ubicacion.Longitud,
                    IsTerminal(p),
                    CountRoutesForParada(p, rutas, null)))
                .ToList();
        }

        if (includeDestinos)
        {
            var destinos = (await _store.ListDestinosAsync(ct).ConfigureAwait(false)).Where(d => d.Activo);
            if (!string.IsNullOrWhiteSpace(categoriaDestino))
            {
                var c = Normalize(categoriaDestino);
                destinos = destinos.Where(d => Normalize(d.Categoria).Contains(c, StringComparison.Ordinal));
            }

            destinos = destinos.Where(d => InsideArea(
                d.Ubicacion.Latitud,
                d.Ubicacion.Longitud,
                minLat,
                minLng,
                maxLat,
                maxLng,
                lat,
                lon,
                radioKm));

            destinosResult = destinos
                .Select(d => new MapaDestinoItemDto(
                    d.Id,
                    d.Nombre,
                    d.Categoria,
                    d.Ubicacion.Latitud,
                    d.Ubicacion.Longitud,
                    d.CalificacionPromedio))
                .ToList();
        }

        return new MapaElementosDetalleResponse(paradasResult, destinosResult);
    }

    // ---------------------------------------------------------------------
    // EP4: horarios y tarifas
    // ---------------------------------------------------------------------
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

        return list.Select(MapHorario).ToList();
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

        return list.Select(MapTarifa).ToList();
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

    // ---------------------------------------------------------------------
    // EP5: destinos turísticos
    // ---------------------------------------------------------------------
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

    // ---------------------------------------------------------------------
    // Helpers de proyección
    // ---------------------------------------------------------------------
    private RutaCatalogoItemDto MapRutaCatalogo(RutaDoc r) =>
        new(
            r.Id,
            r.Codigo,
            r.Nombre,
            r.EmpresaNombre,
            r.MedioTransporteNombre,
            r.ParadaOrigen,
            r.ParadaDestino,
            r.Paradas.Count,
            r.TarifaBaseMxn,
            r.FechaActualizacion ?? _store.CatalogUpdatedAtUtc);

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
            r.DurasionSegura());

    private static ParadaListItemDto MapParadaListItem(ParadaDoc p, double? distanciaMetros) =>
        new(
            p.Id,
            p.Codigo ?? p.Id,
            p.Nombre,
            p.Referencias,
            p.Municipio,
            IsTerminal(p),
            p.Ubicacion.Latitud,
            p.Ubicacion.Longitud,
            distanciaMetros);

    private static ParadaDetalleDto MapParadaDetalle(ParadaDoc p, IReadOnlyList<RutaQuePasaDto> rutas) =>
        new(
            p.Id,
            p.Codigo ?? p.Id,
            p.Nombre,
            p.Referencias,
            p.Municipio,
            IsTerminal(p),
            p.Ubicacion.Latitud,
            p.Ubicacion.Longitud,
            rutas);

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

    private static HorarioItemDto MapHorario(HorarioDoc h) =>
        new(h.Id, h.RutaId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.FrecuenciaMin, h.SalidasFijas, h.Activo);

    private static TarifaItemDto MapTarifa(TarifaDoc t) =>
        new(t.Id, t.RutaId, t.TipoTarifa, t.MontoMxn, t.DescuentoEstudiantePct, t.Moneda, t.Vigente, t.Notas);

    // ---------------------------------------------------------------------
    // Helpers de secuencia y búsqueda
    // ---------------------------------------------------------------------
    [SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Construcción de la línea de tiempo de paradas con filtros de ascenso/descenso (EP-RP-08).")]
    private static List<RutaParadaSecuenciaDto> BuildStopSequence(
        RutaDoc ruta,
        IReadOnlyList<ParadaDoc> paradas,
        bool? soloAscenso,
        bool? soloDescenso)
    {
        var stops = ruta.Paradas.OrderBy(s => s.OrdenSecuencia).ToList();
        var total = stops.Count;
        var result = new List<RutaParadaSecuenciaDto>(total);

        for (var i = 0; i < total; i++)
        {
            var stop = stops[i];
            var parada = FindParada(paradas, stop.ParadaId, stop.Nombre);
            var ascenso = stop.PermiteAscenso;
            var descenso = stop.PermiteDescenso;

            if (soloAscenso == true && !ascenso)
            {
                continue;
            }

            if (soloDescenso == true && !descenso)
            {
                continue;
            }

            var tiempo = stop.TiempoDesdeOrigenMin;
            if (tiempo <= 0 && i > 0 && ruta.DuracionEstimadaMin > 0 && total > 1)
            {
                tiempo = (int)Math.Round(ruta.DuracionEstimadaMin * (double)i / (total - 1), MidpointRounding.AwayFromZero);
            }

            var distancia = stop.DistanciaDesdeOrigenKm;
            if (distancia <= 0 && i > 0 && ruta.DistanciaKm > 0 && total > 1)
            {
                distancia = Math.Round(ruta.DistanciaKm * i / (total - 1), 2);
            }

            result.Add(new RutaParadaSecuenciaDto(
                stop.OrdenSecuencia > 0 ? stop.OrdenSecuencia : i + 1,
                parada?.Id ?? stop.ParadaId,
                parada?.Nombre ?? stop.Nombre,
                parada?.Referencias ?? stop.ReferenciaVisual,
                parada?.Municipio ?? stop.Municipio,
                parada?.Ubicacion.Latitud ?? stop.Latitud,
                parada?.Ubicacion.Longitud ?? stop.Longitud,
                Math.Max(0, tiempo),
                Math.Max(0, distancia),
                ascenso,
                descenso));
        }

        return result;
    }

    private static int? FindStopIndex(
        List<ParadaEnRutaDoc> stops,
        string? paradaId,
        string? nombre)
    {
        if (stops.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(paradaId))
        {
            for (var i = 0; i < stops.Count; i++)
            {
                if (string.Equals(stops[i].ParadaId, paradaId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            for (var i = 0; i < stops.Count; i++)
            {
                if (string.Equals(stops[i].Nombre, paradaId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var n = Normalize(nombre);
            for (var i = 0; i < stops.Count; i++)
            {
                if (Normalize(stops[i].Nombre).Contains(n, StringComparison.Ordinal)
                    || n.Contains(Normalize(stops[i].Nombre), StringComparison.Ordinal))
                {
                    return i;
                }
            }
        }

        return null;
    }

    private static ParadaDoc? FindParada(IReadOnlyList<ParadaDoc> paradas, string? id, string? nombre)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            var byId = paradas.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
            if (byId is not null)
            {
                return byId;
            }
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var normalized = Normalize(nombre);
            return paradas.FirstOrDefault(p => Normalize(p.Nombre) == normalized)
                ?? paradas.FirstOrDefault(p => Normalize(p.Nombre).Contains(normalized, StringComparison.Ordinal));
        }

        return null;
    }

    private static ParadaDoc? ResolveRouteParada(
        RutaDoc ruta,
        IReadOnlyList<ParadaDoc> paradas,
        string? id,
        string? nombre,
        bool first)
    {
        var direct = FindParada(paradas, id, nombre);
        if (direct is not null)
        {
            return direct;
        }

        var stops = ruta.Paradas.OrderBy(s => s.OrdenSecuencia).ToList();
        if (stops.Count == 0)
        {
            return null;
        }

        var stop = first ? stops[0] : stops[^1];
        return FindParada(paradas, stop.ParadaId, stop.Nombre);
    }

    private static bool StopMatches(ParadaEnRutaDoc stop, ParadaDoc parada) =>
        string.Equals(stop.ParadaId, parada.Id, StringComparison.OrdinalIgnoreCase)
        || Normalize(stop.Nombre) == Normalize(parada.Nombre);

    private static int CountRoutesForParada(ParadaDoc parada, IReadOnlyList<RutaDoc> rutas, Guid? rutaId) =>
        rutas.Count(r =>
            (!rutaId.HasValue || r.Id == rutaId.Value)
            && (r.Paradas.Any(st => StopMatches(st, parada))
                || Normalize(r.ParadaOrigen) == Normalize(parada.Nombre)
                || Normalize(r.ParadaDestino) == Normalize(parada.Nombre)));

    private static bool IsOriginOfAnyRoute(ParadaDoc parada, IReadOnlyList<RutaDoc> rutas, Guid? rutaId)
    {
        if (rutaId.HasValue)
        {
            var ruta = rutas.FirstOrDefault(r => r.Id == rutaId.Value);
            if (ruta is null)
            {
                return false;
            }

            var firstStop = ruta.Paradas.OrderBy(s => s.OrdenSecuencia).FirstOrDefault();
            return firstStop is not null && StopMatches(firstStop, parada);
        }

        return rutas.Any(r =>
        {
            if (string.Equals(r.ParadaOrigenId, parada.Id, StringComparison.OrdinalIgnoreCase)
                || Normalize(r.ParadaOrigen) == Normalize(parada.Nombre))
            {
                return true;
            }

            var firstStop = r.Paradas.OrderBy(s => s.OrdenSecuencia).FirstOrDefault();
            return firstStop is not null && StopMatches(firstStop, parada);
        });
    }

    private static int ComputeTramoTime(
        RutaDoc ruta,
        List<ParadaEnRutaDoc> stops,
        int? origenIdx,
        int? destinoIdx)
    {
        if (origenIdx.HasValue && destinoIdx.HasValue)
        {
            var from = stops[origenIdx.Value];
            var to = stops[destinoIdx.Value];
            if (to.TiempoDesdeOrigenMin > from.TiempoDesdeOrigenMin)
            {
                return to.TiempoDesdeOrigenMin - from.TiempoDesdeOrigenMin;
            }
        }

        return ruta.DuracionEstimadaMin;
    }

    private static decimal ComputeTramoFare(
        RutaDoc ruta,
        List<ParadaEnRutaDoc> stops,
        int? origenIdx,
        int? destinoIdx)
    {
        if (!origenIdx.HasValue || !destinoIdx.HasValue || stops.Count <= 1)
        {
            return ruta.TarifaBaseMxn;
        }

        var fraction = (double)(destinoIdx.Value - origenIdx.Value) / (stops.Count - 1);
        var amount = ruta.TarifaBaseMxn * (decimal)fraction;
        return Math.Round(amount, 2);
    }

    private string? ComputeNextDeparture(RutaDoc ruta, IReadOnlyList<HorarioDoc> horarios, string? horaSalida)
    {
        var now = TimeOnly.FromDateTime(_time.GetUtcNow().UtcDateTime.AddHours(-6));
        if (!string.IsNullOrWhiteSpace(horaSalida)
            && TimeOnly.TryParseExact(horaSalida.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            now = parsed;
        }

        var routeSchedules = horarios.Where(h => h.Activo && h.RutaId == ruta.Id).ToList();
        var departures = routeSchedules
            .SelectMany(h => h.SalidasFijas)
            .Select(s => TimeOnly.TryParseExact(s, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? (TimeOnly?)t : null)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .OrderBy(t => t)
            .ToList();

        if (departures.Count == 0)
        {
            foreach (var schedule in routeSchedules)
            {
                if (!TimeOnly.TryParseExact(schedule.HoraInicio, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start)
                    || !TimeOnly.TryParseExact(schedule.HoraFin, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end)
                    || schedule.FrecuenciaMin <= 0)
                {
                    continue;
                }

                for (var t = start; t <= end; t = t.AddMinutes(schedule.FrecuenciaMin))
                {
                    departures.Add(t);
                }
            }
        }

        if (departures.Count == 0)
        {
            return null;
        }

        var next = departures.FirstOrDefault(t => t >= now);
        if (next == default)
        {
            next = departures[0];
        }

        return next.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    private static bool DayMatches(RutaDoc ruta, string diaOperacion, IReadOnlyList<HorarioDoc> horarios)
    {
        var d = Normalize(diaOperacion);
        var routeDays = Normalize(ruta.DiasOperacion);
        var schedules = horarios.Where(h => h.Activo && h.RutaId == ruta.Id).Select(h => Normalize(h.DiaSemana));

        return d switch
        {
            "lv" or "lunvie" or "lunesaviernes" => routeDays.Contains("lunesaviernes", StringComparison.Ordinal)
                || schedules.Any(x => x.Contains("lunesaviernes", StringComparison.Ordinal)),
            "sab" or "sabado" => routeDays.Contains("sabado", StringComparison.Ordinal)
                || schedules.Any(x => x.Contains("sabado", StringComparison.Ordinal)),
            "dom" or "domingo" => routeDays.Contains("domingo", StringComparison.Ordinal)
                || schedules.Any(x => x.Contains("domingo", StringComparison.Ordinal)),
            _ => routeDays.Contains(d, StringComparison.Ordinal)
                || schedules.Any(x => x.Contains(d, StringComparison.Ordinal)),
        };
    }

    private static bool InsideArea(
        double paradaLat,
        double paradaLng,
        double? minLat,
        double? minLng,
        double? maxLat,
        double? maxLng,
        double? lat,
        double? lon,
        double radioKm)
    {
        if (minLat.HasValue && minLng.HasValue && maxLat.HasValue && maxLng.HasValue)
        {
            return paradaLat >= minLat.Value && paradaLat <= maxLat.Value
                && paradaLng >= minLng.Value && paradaLng <= maxLng.Value;
        }

        if (lat.HasValue && lon.HasValue)
        {
            return GeoMath.DistanceKm(lat.Value, lon.Value, paradaLat, paradaLng) <= radioKm;
        }

        return true;
    }

    private static void ValidateCoordinates(double? lat, double? lon)
    {
        if (lat.HasValue && lat.Value is < -90 or > 90)
        {
            throw new RequestValidationException("Latitud fuera de rango válido [-90, 90].");
        }

        if (lon.HasValue && lon.Value is < -180 or > 180)
        {
            throw new RequestValidationException("Longitud fuera de rango válido [-180, 180].");
        }
    }

    private static bool IsTerminal(ParadaDoc parada) =>
        parada.EsTerminal || string.Equals(parada.Tipo, "Terminal", StringComparison.OrdinalIgnoreCase);

    /// <summary>Normaliza texto para búsquedas sin acentos, mayúsculas ni separadores.</summary>
    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark
                || char.IsWhiteSpace(ch)
                || ch is '-' or '_' or '.')
            {
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }
}

internal static class RutaDocExtensions
{
    public static int DurasionSegura(this RutaDoc ruta) =>
        ruta.DuracionEstimadaMin > 0 ? ruta.DuracionEstimadaMin : 30;
}
