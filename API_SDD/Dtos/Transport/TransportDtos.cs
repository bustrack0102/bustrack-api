using System.Diagnostics.CodeAnalysis;

namespace ApiSDD.Dtos.Transport;

public sealed record PagedRutasResponse(
    int totalRegistros,
    int paginaActual,
    int tamanoPagina,
    IReadOnlyList<RutaListItemDto> items);

public sealed record RutaListItemDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string Empresa,
    string MedioTransporte,
    string ParadaOrigen,
    string ParadaDestino,
    decimal TarifaBaseMxn,
    string PrimerHorario,
    string UltimoHorario,
    int FrecuenciaMinutos,
    string DiasOperacion,
    bool Activo,
    double CalificacionPromedio = 4.5,
    int DuracionEstimadaMin = 30);

public sealed record AutocompleteItemDto(string Id, string Nombre, string Subtitulo);

public sealed record RutaParadaItemDto(
    string ParadaId,
    string Nombre,
    double Latitud,
    double Longitud,
    int OrdenSecuencia);

public sealed record RutaParadasResponse(
    Guid rutaId,
    string rutaNombre,
    int totalParadas,
    IReadOnlyList<RutaParadaItemDto> paradas);

public sealed record RutaGeometriaResponse(
    Guid RutaId,
    string TipoGeometria,
    IReadOnlyList<double[]> Coordenadas);

public sealed record ParadaDetallePublicoDto(
    string Id,
    string Nombre,
    string Municipio,
    double Latitud,
    double Longitud,
    bool Techada,
    int RutasDisponibles,
    string? Referencias = null);

public sealed record ParadaCercanaDto(
    string Id,
    string Nombre,
    string Municipio,
    double Latitud,
    double Longitud,
    int RutasDisponibles,
    double DistanciaAproximadaKm);

public sealed record MapaElementoDto(
    string Id,
    string Tipo,
    string Nombre,
    string Municipio,
    double Latitud,
    double Longitud,
    string? Categoria = null);

public sealed record MapaElementosResponse(
    int TotalElementos,
    IReadOnlyList<MapaElementoDto> Elementos);

public sealed record HorarioItemDto(
    string Id,
    Guid RutaId,
    string DiaSemana,
    string HoraInicio,
    string HoraFin,
    int FrecuenciaMin,
    IReadOnlyList<string> SalidasFijas,
    bool Activo);

public sealed record ProximaSalidaResponse(
    Guid RutaId,
    string HoraConsulta,
    string? ProximaSalida,
    int MinutosRestantes,
    int FrecuenciaMin);

public sealed record TarifaItemDto(
    string Id,
    Guid RutaId,
    string TipoTarifa,
    decimal MontoMxn,
    int DescuentoEstudiantePct,
    string Moneda,
    bool Vigente,
    string? Notas);

public sealed record CalculoTarifaResponse(
    Guid RutaId,
    string TipoPasajero,
    decimal TarifaRegularMxn,
    decimal TarifaFinalMxn,
    int DescuentoAplicadoPct,
    string Moneda);

public sealed record DestinoTuristicoDto(
    string Id,
    string Nombre,
    string Categoria,
    string Municipio,
    string Descripcion,
    string ParadaCercanaId,
    double Latitud,
    double Longitud,
    double CalificacionPromedio,
    IReadOnlyList<Guid> RutasIds,
    bool Activo);

public sealed record DestinoCategoriaCountDto(string Categoria, int Total);

[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
public sealed record EstadoActualizacionCatalogosResponse(
    string VersionCatalogo,
    DateTime ActualizadoEnUtc,
    int TotalEmpresas,
    int TotalRutas,
    int TotalParadas,
    int TotalDestinos);
