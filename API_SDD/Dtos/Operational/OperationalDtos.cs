using System.Diagnostics.CodeAnalysis;

namespace ApiSDD.Dtos.Operational;

// --- EP6 Calificaciones (EP-CA-01..06) ---
[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
public sealed record CalificacionCreateRequest(
    Guid? RutaId,
    Guid? EmpresaId,
    string? DestinoId,
    int Puntuacion,
    string? Comentario,
    string? ImagenUrl);

[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
public sealed record CalificacionUpdateRequest(
    int Puntuacion,
    string? Comentario,
    string? ImagenUrl);

[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
public sealed record CalificacionItemDto(
    string Id,
    Guid UsuarioId,
    string UsuarioNombre,
    Guid? RutaId,
    Guid? EmpresaId,
    string? DestinoId,
    int Puntuacion,
    string? Comentario,
    string? ImagenUrl,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);

public sealed record PagedCalificacionesResponse(
    int TotalRegistros,
    int PaginaActual,
    int TamanoPagina,
    IReadOnlyList<CalificacionItemDto> Items);

public sealed record CalificacionResumenDto(
    int TotalOpiniones,
    double Promedio,
    int Estrellas5,
    int Estrellas4,
    int Estrellas3,
    int Estrellas2,
    int Estrellas1);

// --- EP7 Viajes Guardados (EP-VG-01..05) ---
public sealed record ViajeCreateRequest(
    string Titulo,
    Guid RutaId,
    string? ParadaOrigenId,
    string? ParadaDestinoId,
    string? Notas,
    bool Favorito = false);

public sealed record ViajeUpdateRequest(
    string Titulo,
    string? ParadaOrigenId,
    string? ParadaDestinoId,
    string? Notas,
    bool Favorito);

public sealed record ViajeItemDto(
    string Id,
    string Titulo,
    Guid RutaId,
    string RutaNombre,
    string? ParadaOrigenId,
    string? ParadaDestinoId,
    string? Notas,
    bool Favorito,
    DateTime FechaActualizacion);

public sealed record ViajeDetalleDto(
    string Id,
    string Titulo,
    Guid RutaId,
    string RutaNombre,
    string EmpresaNombre,
    string? ParadaOrigenId,
    string? ParadaDestinoId,
    decimal TarifaEstimadaMxn,
    int TotalParadas,
    string? Notas,
    bool Favorito,
    DateTime FechaActualizacion);

// --- Notificaciones Push FCM ---
public sealed record DispositivoRegisterRequest(
    string TokenFcm,
    string Plataforma = "android");

public sealed record DispositivoItemDto(
    string Id,
    string Plataforma,
    string Sufijo,
    DateTime ActualizadoEnUtc);

// --- EP10 Administración: Fuentes de Datos e Importaciones (EP-AD-02..07) ---
[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
public sealed record FuenteDatoUpsertRequest(
    string Nombre,
    string TipoFuente,
    string Responsable,
    string? ReferenciaUrl,
    bool Activa = true);

[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
public sealed record FuenteDatoItemDto(
    Guid Id,
    string Nombre,
    string TipoFuente,
    string Responsable,
    string? ReferenciaUrl,
    bool Activa,
    DateTime ActualizadaEnUtc);

public sealed record ImportacionExecuteRequest(
    Guid FuenteDatoId,
    string TipoEntidad,
    int RegistrosProcesados = 1);

public sealed record ImportacionItemDto(
    string Id,
    Guid FuenteDatoId,
    Guid UsuarioAdminId,
    string TipoEntidad,
    int RegistrosProcesados,
    string Estado,
    DateTime EjecutadoEnUtc);

// --- Admin Catálogos CRUD (EP-RP-11..16, EP-HT-03..05/08..10, EP-DE-06..08) ---
public sealed record ParadaUpsertRequest(
    string? Id,
    string Nombre,
    string Municipio,
    double Latitud,
    double Longitud,
    bool Techada = false,
    string? Referencias = null,
    bool Activo = true);

public sealed record RutaUpsertRequest(
    string Codigo,
    string Nombre,
    Guid EmpresaId,
    int MedioTransporteId,
    string ParadaOrigen,
    string ParadaDestino,
    decimal TarifaBaseMxn,
    string PrimerHorario,
    string UltimoHorario,
    int FrecuenciaMinutos,
    string DiasOperacion = "Lun-Dom",
    int DuracionEstimadaMin = 30,
    IReadOnlyList<string>? ParadasIds = null,
    IReadOnlyList<double[]>? Coordenadas = null,
    bool Activa = true);

public sealed record HorarioUpsertRequest(
    Guid RutaId,
    string DiaSemana,
    string HoraInicio,
    string HoraFin,
    int FrecuenciaMin,
    IReadOnlyList<string>? SalidasFijas = null,
    bool Activo = true);

public sealed record TarifaUpsertRequest(
    Guid RutaId,
    string TipoTarifa,
    decimal MontoMxn,
    int DescuentoEstudiantePct = 20,
    string Moneda = "MXN",
    bool Vigente = true,
    string? Notas = null);

public sealed record DestinoUpsertRequest(
    string? Id,
    string Nombre,
    string Categoria,
    string Municipio,
    string Descripcion,
    string ParadaCercanaId,
    double Latitud,
    double Longitud,
    IReadOnlyList<Guid>? RutasIds = null,
    bool Activo = true);
