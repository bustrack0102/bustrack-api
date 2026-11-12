using System.Text.Json.Serialization;

namespace ApiSDD.Dtos.Intelligence;

// --- Consentimientos y Sensores ML (EP-ML-01..07, EP-AD-08) ---
public sealed record ConsentimientoEstadoDto(
    Guid UsuarioId,
    bool Activo,
    string VersionAviso,
    DateTime? FechaActualizacion);

public sealed record OtorgarConsentimientoRequest(
    bool AceptaAviso = true,
    string VersionAviso = "1.0");

public sealed record SensorMuestraDto(
    int TimestampRelativoMs,
    double AccelX,
    double AccelY,
    double AccelZ);

public sealed record LoteSensorCreateRequest(
    string ViajeId,
    Guid RutaId,
    int IntervaloMuestreoMs = 100,
    double? LatitudReferencia = null,
    double? LongitudReferencia = null,
    string? NombreParadaSugerido = null,
    IReadOnlyList<SensorMuestraDto>? Muestras = null);

public sealed record LoteSensorResultadoDto(
    string LoteId,
    string InferenciaId,
    string ViajeAnonimoHash,
    Guid RutaId,
    string ClasificacionPrincipal,
    double ConfianzaGlobal,
    int SeveridadMaxima,
    int TotalMuestrasProcesadas,
    string ProveedorMl,
    string? CandidatoParadaId,
    DateTime CreadoEnUtc);

public sealed record LoteSensorResumenDto(
    string Id,
    string ViajeAnonimoHash,
    Guid RutaId,
    int TotalMuestras,
    string ClasificacionPrincipal,
    double ConfianzaGlobal,
    DateTime CreadoEnUtc);

public sealed record CandidatoParadaItemDto(
    string Id,
    Guid RutaId,
    string LoteId,
    string NombreSugerido,
    string Municipio,
    double Latitud,
    double Longitud,
    double Confianza,
    string Estado,
    DateTime CreadoEnUtc);

public sealed record ValidarCandidatoParadaRequest(
    bool Aprobar,
    string? NombreParadaFinal = null,
    string? Municipio = null,
    string? Notas = null);

public sealed record ValidarCandidatoParadaResponse(
    string CandidatoId,
    string Estado,
    string? ParadaPromovidaId,
    int NotificacionesPushEnviadas);

// --- GenAI Recomendaciones (EP-IA-01..03) ---
public sealed record HistorialRecomendacionItemDto(
    string Id,
    string Origen,
    string Destino,
    string Prioridad,
    Guid? RutaRecomendadaId,
    string Explicacion,
    string ModeloUsado,
    int? UtilidadPuntuacion,
    string? ComentarioFeedback,
    DateTime CreadoEnUtc);

public sealed record RetroalimentacionRecomendacionRequest(
    bool Util,
    int? Puntuacion = null,
    string? Comentario = null);

public sealed record RetroalimentacionRecomendacionResponse(
    string RecomendacionId,
    int UtilidadPuntuacion,
    string? ComentarioFeedback,
    string Mensaje);

// --- Contratos internos hacia bustrack-ml / bustrack-genai ---
public sealed record MlInferenceUpstreamRequest(
    [property: JsonPropertyName("viajeId")] string ViajeId,
    [property: JsonPropertyName("rutaId")] string RutaId,
    [property: JsonPropertyName("intervaloMuestreoMs")] int IntervaloMuestreoMs,
    [property: JsonPropertyName("consentimientoUsuario")] bool ConsentimientoUsuario,
    [property: JsonPropertyName("muestras")] IReadOnlyList<SensorMuestraDto> Muestras);

public sealed record MlInferenceUpstreamResponse(
    [property: JsonPropertyName("inferenciaId")] string InferenciaId,
    [property: JsonPropertyName("viajeAnonimoHash")] string ViajeAnonimoHash,
    [property: JsonPropertyName("rutaId")] string RutaId,
    [property: JsonPropertyName("modeloVersion")] string ModeloVersion,
    [property: JsonPropertyName("clasificacionPrincipal")] string ClasificacionPrincipal,
    [property: JsonPropertyName("confianzaGlobal")] double ConfianzaGlobal,
    [property: JsonPropertyName("severidadMaxima")] int SeveridadMaxima,
    [property: JsonPropertyName("totalMuestrasProcesadas")] int TotalMuestrasProcesadas,
    [property: JsonPropertyName("procesadoEnMs")] int ProcesadoEnMs);

public sealed record GenAiRouteCandidateDto(
    [property: JsonPropertyName("rutaId")] string RutaId,
    [property: JsonPropertyName("lineaNombre")] string LineaNombre,
    [property: JsonPropertyName("empresaId")] string EmpresaId,
    [property: JsonPropertyName("empresaNombre")] string EmpresaNombre,
    [property: JsonPropertyName("medioTransporte")] string MedioTransporte,
    [property: JsonPropertyName("origenNombre")] string OrigenNombre,
    [property: JsonPropertyName("destinoNombre")] string DestinoNombre,
    [property: JsonPropertyName("tarifaMxn")] double TarifaMxn,
    [property: JsonPropertyName("duracionEstimadaMin")] int DuracionEstimadaMin,
    [property: JsonPropertyName("calificacionPromedio")] double CalificacionPromedio,
    [property: JsonPropertyName("horariosDisponibles")] IReadOnlyList<string> HorariosDisponibles,
    [property: JsonPropertyName("paradasOrdenadas")] IReadOnlyList<string> ParadasOrdenadas);

public sealed record GenAiPreferencesDto(
    [property: JsonPropertyName("prioridadBusqueda")] string PrioridadBusqueda,
    [property: JsonPropertyName("idioma")] string Idioma,
    [property: JsonPropertyName("notasAdicionales")] string? NotasAdicionales);

public sealed record GenAiUpstreamRequest(
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("destinoId")] string DestinoId,
    [property: JsonPropertyName("preferencias")] GenAiPreferencesDto Preferencias,
    [property: JsonPropertyName("rutasCandidatas")] IReadOnlyList<GenAiRouteCandidateDto> RutasCandidatas);

public sealed record GenAiUpstreamResponse(
    [property: JsonPropertyName("recomendacionId")] string RecomendacionId,
    [property: JsonPropertyName("rutaRecomendadaId")] string RutaRecomendadaId,
    [property: JsonPropertyName("explicacion")] string Explicacion,
    [property: JsonPropertyName("promptVersion")] string PromptVersion);
