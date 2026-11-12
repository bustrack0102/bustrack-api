using System.Security.Claims;
using System.Text.RegularExpressions;
using ApiSDD.Common;
using ApiSDD.Controllers.V1;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.Integrations;
using ApiSDD.Services.Moderation;
using ApiSDD.Services.Notifications;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Orquestador Etapa 3 para ML Sensores/Consentimientos/Candidatos (EP-ML-01..07, EP-AD-08) y GenAI Grounded (EP-IA-01..03).
/// </summary>
public interface IIntelligenceService
{
    Task<ConsentimientoEstadoDto> GetConsentimientoAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<ConsentimientoEstadoDto> GrantConsentimientoAsync(ClaimsPrincipal user, OtorgarConsentimientoRequest request, CancellationToken ct);
    Task<ConsentimientoEstadoDto> RevokeConsentimientoAsync(ClaimsPrincipal user, CancellationToken ct);

    Task<LoteSensorResultadoDto> SubmitSensorBatchAsync(ClaimsPrincipal user, LoteSensorCreateRequest request, CancellationToken ct);
    Task<IReadOnlyList<LoteSensorResumenDto>> ListLotesAsync(Guid? rutaId, CancellationToken ct);
    Task<IReadOnlyList<CandidatoParadaItemDto>> ListCandidatosParadaAsync(string? estado, Guid? rutaId, double? lat, double? lon, double radioKm, CancellationToken ct);
    Task<ValidarCandidatoParadaResponse> ValidateCandidatoParadaAsync(string id, ValidarCandidatoParadaRequest request, CancellationToken ct);

    Task<TelemetriaInferenciaResponse> InferTelemetriaAsync(TelemetriaInferenciaRequest request, CancellationToken ct);
    Task<RecomendacionResponse> GenerateRecomendacionAsync(ClaimsPrincipal? user, RecomendacionRequest request, CancellationToken ct);
    Task<IReadOnlyList<HistorialRecomendacionItemDto>> ListHistorialRecomendacionesAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<RetroalimentacionRecomendacionResponse> SubmitRetroalimentacionAsync(
        ClaimsPrincipal user,
        string recomendacionId,
        RetroalimentacionRecomendacionRequest request,
        CancellationToken ct);
}

internal sealed partial class IntelligenceService : IIntelligenceService
{
    private const int MaxMuestrasPerBatch = 500;

    private readonly IBusTrackNoSqlStore _store;
    private readonly IMlServiceClient _ml;
    private readonly IGenAiServiceClient _genAi;
    private readonly IContentModerationService _moderation;
    private readonly IPushNotificationService _push;
    private readonly ITransportCatalogService _catalog;
    private readonly TimeProvider _time;

    public IntelligenceService(
        IBusTrackNoSqlStore store,
        IMlServiceClient ml,
        IGenAiServiceClient genAi,
        IContentModerationService moderation,
        IPushNotificationService push,
        ITransportCatalogService catalog,
        TimeProvider time)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _ml = ml ?? throw new ArgumentNullException(nameof(ml));
        _genAi = genAi ?? throw new ArgumentNullException(nameof(genAi));
        _moderation = moderation ?? throw new ArgumentNullException(nameof(moderation));
        _push = push ?? throw new ArgumentNullException(nameof(push));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    [GeneratedRegex(@"https?://\S+", RegexOptions.IgnoreCase)]
    private static partial Regex ExternalUrlRegex();

    public async Task<ConsentimientoEstadoDto> GetConsentimientoAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetConsentimientoAsync(userId, ct).ConfigureAwait(false);
        if (doc is null)
        {
            var account = await _store.GetUsuarioByIdAsync(userId, ct).ConfigureAwait(false);
            return new ConsentimientoEstadoDto(
                userId,
                account?.ConsentimientoSensores ?? false,
                "1.0",
                account?.FechaAvisoPrivacidad);
        }

        return new ConsentimientoEstadoDto(doc.UsuarioId, doc.Activo, doc.VersionAviso, doc.FechaActualizacion);
    }

    public async Task<ConsentimientoEstadoDto> GrantConsentimientoAsync(
        ClaimsPrincipal user,
        OtorgarConsentimientoRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.AceptaAviso)
        {
            throw new RequestValidationException("Debes aceptar el aviso de privacidad de sensores.");
        }

        var userId = user.GetRequiredUserId();
        var now = _time.GetUtcNow().UtcDateTime;
        var version = string.IsNullOrWhiteSpace(request.VersionAviso) ? "1.0" : request.VersionAviso.Trim();

        var doc = new ConsentimientoSensorDoc
        {
            UsuarioId = userId,
            Activo = true,
            VersionAviso = version,
            FechaActualizacion = now,
        };
        await _store.UpsertConsentimientoAsync(doc, ct).ConfigureAwait(false);

        var account = await _store.GetUsuarioByIdAsync(userId, ct).ConfigureAwait(false);
        if (account is not null)
        {
            account.ConsentimientoSensores = true;
            account.FechaAvisoPrivacidad = now;
            await _store.UpdateUsuarioAsync(account, ct).ConfigureAwait(false);
        }

        return new ConsentimientoEstadoDto(userId, true, version, now);
    }

    public async Task<ConsentimientoEstadoDto> RevokeConsentimientoAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = new ConsentimientoSensorDoc
        {
            UsuarioId = userId,
            Activo = false,
            VersionAviso = "revocada",
            FechaActualizacion = now,
        };
        await _store.UpsertConsentimientoAsync(doc, ct).ConfigureAwait(false);

        var account = await _store.GetUsuarioByIdAsync(userId, ct).ConfigureAwait(false);
        if (account is not null)
        {
            account.ConsentimientoSensores = false;
            account.FechaActualizacion = now;
            await _store.UpdateUsuarioAsync(account, ct).ConfigureAwait(false);
        }

        return new ConsentimientoEstadoDto(userId, false, doc.VersionAviso, now);
    }

    public async Task<LoteSensorResultadoDto> SubmitSensorBatchAsync(
        ClaimsPrincipal user,
        LoteSensorCreateRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = user.GetRequiredUserId();

        var consent = await GetConsentimientoAsync(user, ct).ConfigureAwait(false);
        if (!consent.Activo)
        {
            throw new ForbiddenException("Debes otorgar consentimiento de sensores antes de enviar telemetría.");
        }

        if (request.Muestras is null || request.Muestras.Count == 0 || request.Muestras.Count > MaxMuestrasPerBatch)
        {
            throw new RequestValidationException($"El lote debe contener entre 1 y {MaxMuestrasPerBatch} muestras.");
        }

        var ruta = await _store.GetRutaByIdAsync(request.RutaId, ct).ConfigureAwait(false)
                   ?? throw new NotFoundException("Ruta no encontrada.");

        var mlResp = await _ml.DetectAsync(
            string.IsNullOrWhiteSpace(request.ViajeId) ? $"VJ-{Guid.NewGuid():N}"[..10] : request.ViajeId.Trim(),
            request.RutaId.ToString(),
            Math.Clamp(request.IntervaloMuestreoMs, 10, 5000),
            true,
            request.Muestras,
            ct).ConfigureAwait(false);

        var now = _time.GetUtcNow().UtcDateTime;
        var loteId = $"lot-{Guid.NewGuid():N}";
        var lote = new LoteSensorDoc
        {
            Id = loteId,
            UsuarioHash = mlResp.ViajeAnonimoHash,
            RutaId = request.RutaId,
            TotalMuestras = mlResp.TotalMuestrasProcesadas,
            ClasificacionPrincipal = mlResp.ClasificacionPrincipal,
            ConfianzaGlobal = mlResp.ConfianzaGlobal,
            CreadoEnUtc = now,
        };
        await _store.CreateLoteSensorAsync(lote, ct).ConfigureAwait(false);

        string? candidatoId = null;
        if (mlResp.ClasificacionPrincipal == "PARADA_DETECTADA"
            && request.LatitudReferencia.HasValue
            && request.LongitudReferencia.HasValue)
        {
            candidatoId = $"CAN-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            var cand = new CandidatoParadaDoc
            {
                Id = candidatoId,
                RutaId = request.RutaId,
                LoteId = loteId,
                UsuarioId = userId,
                NombreSugerido = string.IsNullOrWhiteSpace(request.NombreParadaSugerido)
                    ? $"Parada detectada en {ruta.Nombre}"
                    : request.NombreParadaSugerido.Trim(),
                Municipio = ruta.Municipio,
                Ubicacion = GeoJsonPointDoc.FromLatLon(
                    request.LatitudReferencia.Value,
                    request.LongitudReferencia.Value),
                Confianza = mlResp.ConfianzaGlobal,
                Estado = "Pendiente",
                CreadoEnUtc = now,
            };
            await _store.CreateCandidatoParadaAsync(cand, ct).ConfigureAwait(false);
        }

        return new LoteSensorResultadoDto(
            loteId,
            mlResp.InferenciaId,
            mlResp.ViajeAnonimoHash,
            request.RutaId,
            mlResp.ClasificacionPrincipal,
            mlResp.ConfianzaGlobal,
            mlResp.SeveridadMaxima,
            mlResp.TotalMuestrasProcesadas,
            mlResp.ModeloVersion,
            candidatoId,
            now);
    }

    public async Task<IReadOnlyList<LoteSensorResumenDto>> ListLotesAsync(Guid? rutaId, CancellationToken ct)
    {
        var list = await _store.ListLotesSensorAsync(rutaId, ct).ConfigureAwait(false);
        return list.Select(l => new LoteSensorResumenDto(
            l.Id,
            l.UsuarioHash,
            l.RutaId,
            l.TotalMuestras,
            l.ClasificacionPrincipal,
            l.ConfianzaGlobal,
            l.CreadoEnUtc)).ToList();
    }

    public async Task<IReadOnlyList<CandidatoParadaItemDto>> ListCandidatosParadaAsync(
        string? estado,
        Guid? rutaId,
        double? lat,
        double? lon,
        double radioKm,
        CancellationToken ct)
    {
        var items = await _store.ListCandidatosParadaAsync(estado, rutaId, ct).ConfigureAwait(false);
        IEnumerable<CandidatoParadaDoc> filtered = items;
        if (lat.HasValue && lon.HasValue)
        {
            var radius = Math.Clamp(radioKm, 0.2, 50.0);
            filtered = filtered.Where(c =>
                GeoMath.DistanceKm(lat.Value, lon.Value, c.Ubicacion.Latitud, c.Ubicacion.Longitud) <= radius);
        }

        return filtered.Select(c => new CandidatoParadaItemDto(
            c.Id,
            c.RutaId,
            c.LoteId,
            c.NombreSugerido,
            c.Municipio,
            c.Ubicacion.Latitud,
            c.Ubicacion.Longitud,
            c.Confianza,
            c.Estado,
            c.CreadoEnUtc)).ToList();
    }

    public async Task<ValidarCandidatoParadaResponse> ValidateCandidatoParadaAsync(
        string id,
        ValidarCandidatoParadaRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var cand = await _store.GetCandidatoParadaByIdAsync(id, ct).ConfigureAwait(false)
                   ?? throw new NotFoundException("Candidato de parada no encontrado.");

        if (!string.Equals(cand.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException($"El candidato '{id}' ya fue procesado ({cand.Estado}).");
        }

        string? paradaPromovidaId = null;
        var pushSent = 0;

        if (request.Aprobar)
        {
            paradaPromovidaId = $"PAR-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            var parada = new ParadaDoc
            {
                Id = paradaPromovidaId,
                Nombre = string.IsNullOrWhiteSpace(request.NombreParadaFinal) ? cand.NombreSugerido : request.NombreParadaFinal.Trim(),
                Municipio = string.IsNullOrWhiteSpace(request.Municipio) ? cand.Municipio : request.Municipio.Trim(),
                Referencias = request.Notas?.Trim() ?? "Incorporada por validación comunitaria ML.",
                Ubicacion = cand.Ubicacion,
                Techada = false,
                RutasDisponibles = 1,
                Activo = true,
            };
            await _store.UpsertParadaAsync(parada, ct).ConfigureAwait(false);
            cand.Estado = "Aprobada";
            await _store.UpdateCandidatoParadaAsync(cand, ct).ConfigureAwait(false);
            await _catalog.InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);

            var pushRes = await _push.NotifyUserAsync(
                cand.UsuarioId,
                "¡Nueva parada aprobada!",
                $"Tu aporte '{parada.Nombre}' ya forma parte del mapa oficial de BusTrack.",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["candidatoId"] = cand.Id,
                    ["paradaId"] = paradaPromovidaId,
                },
                ct).ConfigureAwait(false);
            pushSent = pushRes.Enviados;
        }
        else
        {
            cand.Estado = "Rechazada";
            await _store.UpdateCandidatoParadaAsync(cand, ct).ConfigureAwait(false);
        }

        return new ValidarCandidatoParadaResponse(cand.Id, cand.Estado, paradaPromovidaId, pushSent);
    }

    public async Task<TelemetriaInferenciaResponse> InferTelemetriaAsync(
        TelemetriaInferenciaRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var muestras = request.Muestras
            .Select(m => new SensorMuestraDto(m.TimestampRelativoMs, m.AccelX, m.AccelY, m.AccelZ))
            .ToList();

        var mlResp = await _ml.DetectAsync(
            request.ViajeId,
            request.RutaId,
            request.IntervaloMuestreoMs > 0 ? request.IntervaloMuestreoMs : 100,
            true,
            muestras,
            ct).ConfigureAwait(false);

        return new TelemetriaInferenciaResponse(
            mlResp.InferenciaId,
            request.ViajeId,
            mlResp.ClasificacionPrincipal,
            mlResp.ConfianzaGlobal,
            mlResp.SeveridadMaxima,
            mlResp.TotalMuestrasProcesadas,
            mlResp.ProcesadoEnMs);
    }

    public async Task<RecomendacionResponse> GenerateRecomendacionAsync(
        ClaimsPrincipal? user,
        RecomendacionRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var modDecision = await _moderation.ModerateTextAsync(
            $"{request.Origen} {request.Destino} {request.PreguntaLibre}",
            "recomendacion",
            ct).ConfigureAwait(false);
        if (!modDecision.Aprobado)
        {
            throw new RequestValidationException(string.Join(" ", modDecision.InfraccionesDetectadas));
        }

        var allRoutes = (await _store.ListRutasAsync(ct).ConfigureAwait(false))
            .Where(r => r.Activa)
            .ToList();

        var candidatos = SelectCandidateRoutes(allRoutes, request);
        var prioridadNormalized = NormalizePriority(request.Preferencia);

        var genAiCandidates = candidatos.Select(r => new GenAiRouteCandidateDto(
            r.Id.ToString(),
            r.Nombre,
            r.EmpresaId.ToString(),
            r.EmpresaNombre,
            r.MedioTransporteNombre,
            r.ParadaOrigen,
            r.ParadaDestino,
            (double)r.TarifaBaseMxn,
            Math.Max(5, r.DuracionEstimadaMin),
            r.CalificacionPromedio,
            [r.PrimerHorario, r.UltimoHorario],
            r.Paradas.Select(p => p.Nombre).ToList())).ToList();

        var userIdOpt = user.TryGetUserId();
        var genAiUpstream = await _genAi.RecommendAsync(
            new GenAiUpstreamRequest(
                userIdOpt?.ToString() ?? "USR-ANON",
                request.Destino ?? "Hidalgo",
                new GenAiPreferencesDto(prioridadNormalized, "es-MX", request.PreguntaLibre),
                genAiCandidates),
            ct).ConfigureAwait(false);

        var (chosenRoute, explicacion, modeloUsado) = ValidateAndResolveGenAiOutput(
            genAiUpstream,
            candidatos,
            prioridadNormalized,
            request);

        var now = _time.GetUtcNow().UtcDateTime;
        var recId = $"REC-{Guid.NewGuid():N}";
        var entity = new SolicitudRecomendacionDoc
        {
            Id = recId,
            UsuarioId = userIdOpt,
            Origen = request.Origen?.Trim() ?? "Tula",
            Destino = request.Destino?.Trim() ?? "Regional",
            Prioridad = prioridadNormalized,
            RutaRecomendadaId = chosenRoute.Id,
            Explicacion = explicacion,
            ModeloUsado = modeloUsado,
            CreadoEnUtc = now,
        };
        await _store.CreateRecomendacionAsync(entity, ct).ConfigureAwait(false);

        var sugeridas = candidatos
            .OrderByDescending(r => r.Id == chosenRoute.Id)
            .Select(r => new RutaSugeridaDto(
                r.Id.ToString(),
                r.Codigo,
                r.Nombre,
                r.TarifaBaseMxn,
                r.DuracionEstimadaMin,
                r.ParadaOrigen,
                r.ParadaDestino))
            .ToList();

        return new RecomendacionResponse(
            recId,
            sugeridas,
            explicacion,
            $"Catálogo Oficial Verificado BusTrack API ({modeloUsado})",
            now);
    }

    public async Task<IReadOnlyList<HistorialRecomendacionItemDto>> ListHistorialRecomendacionesAsync(
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var docs = await _store.ListRecomendacionesAsync(userId, ct).ConfigureAwait(false);
        return docs.Select(d => new HistorialRecomendacionItemDto(
            d.Id,
            d.Origen,
            d.Destino,
            d.Prioridad,
            d.RutaRecomendadaId,
            d.Explicacion,
            d.ModeloUsado,
            d.UtilidadPuntuacion,
            d.ComentarioFeedback,
            d.CreadoEnUtc)).ToList();
    }

    public async Task<RetroalimentacionRecomendacionResponse> SubmitRetroalimentacionAsync(
        ClaimsPrincipal user,
        string recomendacionId,
        RetroalimentacionRecomendacionRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetRecomendacionByIdAsync(recomendacionId, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Recomendación no encontrada.");

        if (doc.UsuarioId.HasValue && doc.UsuarioId.Value != userId && !user.IsAdmin())
        {
            throw new ForbiddenException("No puedes calificar una recomendación de otro usuario.");
        }

        if (!string.IsNullOrWhiteSpace(request.Comentario))
        {
            var mod = await _moderation.ModerateTextAsync(request.Comentario, "feedback", ct).ConfigureAwait(false);
            if (!mod.Aprobado)
            {
                throw new RequestValidationException(string.Join(" ", mod.InfraccionesDetectadas));
            }
        }

        var score = request.Util ? 5 : 1;
        if (request.Puntuacion.HasValue)
        {
            score = Math.Clamp(request.Puntuacion.Value, 1, 5);
        }

        doc.UtilidadPuntuacion = score;
        doc.ComentarioFeedback = request.Comentario?.Trim();
        await _store.UpdateRecomendacionAsync(doc, ct).ConfigureAwait(false);

        return new RetroalimentacionRecomendacionResponse(
            doc.Id,
            score,
            doc.ComentarioFeedback,
            "Retroalimentación registrada para mejora continua.");
    }

    private static List<RutaDoc> SelectCandidateRoutes(List<RutaDoc> allRoutes, RecomendacionRequest req)
    {
        if (allRoutes.Count == 0)
        {
            throw new RequestValidationException("No hay rutas activas en el catálogo para generar recomendaciones.");
        }

        var matches = allRoutes.Where(r =>
            (!string.IsNullOrWhiteSpace(req.Origen) && (r.Nombre.Contains(req.Origen, StringComparison.OrdinalIgnoreCase)
                                                         || r.ParadaOrigen.Contains(req.Origen, StringComparison.OrdinalIgnoreCase)
                                                         || r.Municipio.Contains(req.Origen, StringComparison.OrdinalIgnoreCase)))
            || (!string.IsNullOrWhiteSpace(req.Destino) && (r.Nombre.Contains(req.Destino, StringComparison.OrdinalIgnoreCase)
                                                            || r.ParadaDestino.Contains(req.Destino, StringComparison.OrdinalIgnoreCase)))).ToList();

        return (matches.Count > 0 ? matches : allRoutes).Take(5).ToList();
    }

    private static string NormalizePriority(string? preferencia) =>
        (preferencia ?? "equilibrado").Trim().ToUpperInvariant() switch
        {
            "MENOR_TIEMPO" => "menor_tiempo",
            "MENOR_COSTO" => "menor_costo",
            "MEJOR_CALIFICACION" => "mejor_calificacion",
            "BALANCEADO" or "DIRECTO" or "EQUILIBRADO" => "equilibrado",
            _ => "equilibrado",
        };

    // Validador Anti-Alucinación BE-34: verifica estrictamente contra catálogo real antes de entregar al cliente.
    private static (RutaDoc Route, string Explicacion, string Modelo) ValidateAndResolveGenAiOutput(
        GenAiUpstreamResponse? upstream,
        List<RutaDoc> candidates,
        string priority,
        RecomendacionRequest req)
    {
        var byId = candidates.ToDictionary(r => r.Id.ToString(), StringComparer.OrdinalIgnoreCase);
        if (upstream is not null
            && byId.TryGetValue(upstream.RutaRecomendadaId, out var matched)
            && !string.IsNullOrWhiteSpace(upstream.Explicacion)
            && !ExternalUrlRegex().IsMatch(upstream.Explicacion))
        {
            return (matched, upstream.Explicacion, $"bustrack-genai:{upstream.PromptVersion}");
        }

        var chosen = priority switch
        {
            "menor_tiempo" => candidates.OrderBy(r => r.DuracionEstimadaMin).ThenBy(r => r.TarifaBaseMxn).First(),
            "menor_costo" => candidates.OrderBy(r => r.TarifaBaseMxn).ThenBy(r => r.DuracionEstimadaMin).First(),
            "mejor_calificacion" => candidates.OrderByDescending(r => r.CalificacionPromedio).First(),
            _ => candidates.OrderBy(r => (r.DuracionEstimadaMin * 0.4) + ((double)r.TarifaBaseMxn * 0.4)).First(),
        };

        var groundedText = string.IsNullOrWhiteSpace(req.PreguntaLibre)
            ? $"Se recomienda la ruta {chosen.Codigo} ({chosen.Nombre}) de {chosen.EmpresaNombre}, con tarifa verificada de ${chosen.TarifaBaseMxn:F2} MXN, duración aproximada de {chosen.DuracionEstimadaMin} min y salidas desde {chosen.PrimerHorario}."
            : $"Respuesta verificada en catálogo BusTrack: la ruta {chosen.Codigo} ({chosen.Nombre}) conecta {chosen.ParadaOrigen} con {chosen.ParadaDestino} por ${chosen.TarifaBaseMxn:F2} MXN cada {chosen.FrecuenciaMinutos} min.";

        return (chosen, groundedText, "CatalogGroundedEngine-v1");
    }
}
