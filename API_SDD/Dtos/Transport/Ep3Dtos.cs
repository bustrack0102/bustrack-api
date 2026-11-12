namespace ApiSDD.Dtos.Transport;

// --- EP-RP-01: Autocompletado de paradas ---
public sealed record ParadaAutocompleteItemDto(
    string Id,
    string Nombre,
    string? ReferenciaVisual,
    string Municipio,
    bool EsTerminal,
    double Latitud,
    double Longitud,
    int TotalRutasQuePasan);

// --- EP-RP-02: Listado y filtro de paradas ---
public sealed record ParadaListItemDto(
    string Id,
    string? Codigo,
    string Nombre,
    string? ReferenciaVisual,
    string Municipio,
    bool EsTerminal,
    double Latitud,
    double Longitud,
    double? DistanciaMetros);

public sealed record PagedParadasResponse(
    int Total,
    IReadOnlyList<ParadaListItemDto> Items);

// --- EP-RP-03: Detalle de parada ---
public sealed record RutaQuePasaDto(
    Guid RutaId,
    string Codigo,
    string Nombre,
    string EmpresaNombre,
    string MedioTransporte,
    int OrdenEnRuta);

public sealed record ParadaDetalleDto(
    string Id,
    string? Codigo,
    string Nombre,
    string? ReferenciaVisual,
    string Municipio,
    bool EsTerminal,
    double Latitud,
    double Longitud,
    IReadOnlyList<RutaQuePasaDto> RutasQuePasan);

// --- EP-RP-04: Búsqueda de alternativas ---
public sealed record RutaBusquedaEmpresaDto(Guid Id, string Nombre, double Calificacion);

public sealed record RutaBusquedaAlternativaDto(
    Guid RutaId,
    string Codigo,
    string NombreRuta,
    RutaBusquedaEmpresaDto Empresa,
    string MedioTransporte,
    string? ParadaSubida,
    string? ParadaBajada,
    int CantidadParadasTramo,
    int TiempoEstimadoMin,
    decimal TarifaAproximadaMxn,
    string? SiguienteHorario,
    DateTime FechaActualizacion);

public sealed record RutaBusquedaResponse(
    string? Origen,
    string? Destino,
    int TotalAlternativas,
    string? MensajeSinResultados,
    IReadOnlyList<RutaBusquedaAlternativaDto> Alternativas);

// --- EP-RP-05: Autocompletado de rutas ---
public sealed record RutaAutocompleteItemDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string EmpresaNombre,
    string MedioTransporteNombre,
    string? ParadaOrigenNombre,
    string? ParadaDestinoNombre);

// --- EP-RP-06: Catálogo de rutas ---
public sealed record RutaCatalogoItemDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string Empresa,
    string MedioTransporte,
    string ParadaOrigen,
    string ParadaDestino,
    int TotalParadas,
    decimal TarifaBaseMxn,
    DateTime FechaActualizacion);

public sealed record PagedRutasCatalogoResponse(
    int totalRegistros,
    int pagina,
    IReadOnlyList<RutaCatalogoItemDto> items);

// --- EP-RP-07: Detalle de ruta ---
public sealed record RutaDetalleEmpresaDto(Guid Id, string Nombre, double Calificacion);

public sealed record RutaDetalleMedioTransporteDto(int Id, string Nombre);

public sealed record RutaDetalleParadaDto(
    string Id,
    string Nombre,
    double Latitud,
    double Longitud);

public sealed record RutaParadaSecuenciaDto(
    int Orden,
    string ParadaId,
    string Nombre,
    string? ReferenciaVisual,
    string? Municipio,
    double Latitud,
    double Longitud,
    int TiempoDesdeOrigenMin,
    double DistanciaDesdeOrigenKm,
    bool PermiteAscenso,
    bool PermiteDescenso);

public sealed record RutaDetalleResponse(
    Guid Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    double DistanciaTotalKm,
    int TiempoTotalEstimadoMin,
    RutaDetalleEmpresaDto Empresa,
    RutaDetalleMedioTransporteDto MedioTransporte,
    RutaDetalleParadaDto? ParadaOrigen,
    RutaDetalleParadaDto? ParadaDestino,
    IReadOnlyList<RutaParadaSecuenciaDto> ParadasEnOrden,
    IReadOnlyList<HorarioItemDto> Horarios,
    IReadOnlyList<TarifaItemDto> Tarifas,
    IReadOnlyList<DestinoTuristicoDto> DestinosCercanos,
    DateTime FechaUltimaActualizacion);

// --- EP-RP-09: Geometría GeoJSON ---
public sealed record GeoJsonLineStringDto(string Type, IReadOnlyList<double[]> Coordinates);

public sealed record GeoJsonPointGeometryDto(string Type, IReadOnlyList<double> Coordinates);

public sealed record GeoJsonFeaturePropertiesDto(
    string? Tipo,
    string? Id,
    string? Nombre,
    int? Orden,
    bool? Resaltada,
    string? Categoria);

public sealed record GeoJsonFeatureDto(
    string Type,
    GeoJsonPointGeometryDto Geometry,
    GeoJsonFeaturePropertiesDto Properties);

public sealed record RutaGeometriaDetalleDto(
    Guid RutaId,
    string Nombre,
    IReadOnlyList<double> BoundingBox,
    GeoJsonLineStringDto TrazoGeoJson,
    IReadOnlyList<GeoJsonFeatureDto> ParadasGeoJson,
    IReadOnlyList<GeoJsonFeatureDto> DestinosGeoJson);

// --- EP-RP-10: Elementos del mapa ---
public sealed record MapaParadaItemDto(
    string Id,
    string Nombre,
    double Lat,
    double Lng,
    bool EsTerminal,
    int TotalRutas);

public sealed record MapaDestinoItemDto(
    string Id,
    string Nombre,
    string Categoria,
    double Lat,
    double Lng,
    double CalificacionPromedio);

public sealed record MapaElementosDetalleResponse(
    IReadOnlyList<MapaParadaItemDto> Paradas,
    IReadOnlyList<MapaDestinoItemDto> Destinos);
