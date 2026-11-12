using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Operational;
using ApiSDD.Services.Moderation;
using ApiSDD.Services.Notifications;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Servicio de operación de usuario: Calificaciones moderadas (EP-CA-01..06) y Viajes guardados (EP-VG-01..05).
/// </summary>
public interface IUserInteractionService
{
    Task<PagedCalificacionesResponse> ListCalificacionesAsync(
        Guid? rutaId,
        Guid? empresaId,
        string? destinoId,
        int? puntuacionMin,
        int pagina,
        int tamanoPagina,
        CancellationToken ct);

    Task<CalificacionResumenDto> GetResumenCalificacionesAsync(
        Guid? rutaId,
        Guid? empresaId,
        string? destinoId,
        CancellationToken ct);

    Task<IReadOnlyList<CalificacionItemDto>> ListMisCalificacionesAsync(ClaimsPrincipal user, CancellationToken ct);

    Task<CalificacionItemDto> CreateCalificacionAsync(ClaimsPrincipal user, CalificacionCreateRequest request, CancellationToken ct);

    Task<CalificacionItemDto> UpdateCalificacionAsync(ClaimsPrincipal user, string id, CalificacionUpdateRequest request, CancellationToken ct);

    Task DeleteCalificacionAsync(ClaimsPrincipal user, string id, CancellationToken ct);

    Task<IReadOnlyList<ViajeItemDto>> ListViajesAsync(ClaimsPrincipal user, bool soloFavoritos, string? q, CancellationToken ct);

    Task<ViajeDetalleDto> GetViajeDetalleAsync(ClaimsPrincipal user, string id, CancellationToken ct);

    Task<ViajeItemDto> CreateViajeAsync(ClaimsPrincipal user, ViajeCreateRequest request, CancellationToken ct);

    Task<ViajeItemDto> UpdateViajeAsync(ClaimsPrincipal user, string id, ViajeUpdateRequest request, CancellationToken ct);

    Task DeleteViajeAsync(ClaimsPrincipal user, string id, CancellationToken ct);
}

internal sealed class UserInteractionService : IUserInteractionService
{
    private readonly IBusTrackNoSqlStore _store;
    private readonly IContentModerationService _moderation;
    private readonly IPushNotificationService _push;
    private readonly ITransportCatalogService _catalog;
    private readonly TimeProvider _time;

    public UserInteractionService(
        IBusTrackNoSqlStore store,
        IContentModerationService moderation,
        IPushNotificationService push,
        ITransportCatalogService catalog,
        TimeProvider time)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _moderation = moderation ?? throw new ArgumentNullException(nameof(moderation));
        _push = push ?? throw new ArgumentNullException(nameof(push));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public async Task<PagedCalificacionesResponse> ListCalificacionesAsync(
        Guid? rutaId,
        Guid? empresaId,
        string? destinoId,
        int? puntuacionMin,
        int pagina,
        int tamanoPagina,
        CancellationToken ct)
    {
        var p = Math.Max(1, pagina);
        var s = Math.Clamp(tamanoPagina, 1, 100);
        var docs = await _store.ListCalificacionesAsync(rutaId, empresaId, destinoId, null, ct).ConfigureAwait(false);
        var filtered = docs.AsEnumerable();
        if (puntuacionMin.HasValue)
        {
            filtered = filtered.Where(c => c.Puntuacion >= puntuacionMin.Value);
        }

        var list = filtered.Select(MapCalificacion).ToList();
        return new PagedCalificacionesResponse(
            list.Count,
            p,
            s,
            list.Skip((p - 1) * s).Take(s).ToList());
    }

    public async Task<CalificacionResumenDto> GetResumenCalificacionesAsync(
        Guid? rutaId,
        Guid? empresaId,
        string? destinoId,
        CancellationToken ct)
    {
        var docs = await _store.ListCalificacionesAsync(rutaId, empresaId, destinoId, null, ct).ConfigureAwait(false);
        if (docs.Count == 0)
        {
            return new CalificacionResumenDto(0, 0, 0, 0, 0, 0, 0);
        }

        var avg = Math.Round(docs.Average(c => c.Puntuacion), 2);
        return new CalificacionResumenDto(
            docs.Count,
            avg,
            docs.Count(c => c.Puntuacion == 5),
            docs.Count(c => c.Puntuacion == 4),
            docs.Count(c => c.Puntuacion == 3),
            docs.Count(c => c.Puntuacion == 2),
            docs.Count(c => c.Puntuacion == 1));
    }

    public async Task<IReadOnlyList<CalificacionItemDto>> ListMisCalificacionesAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var docs = await _store.ListCalificacionesAsync(null, null, null, userId, ct).ConfigureAwait(false);
        return docs.Select(MapCalificacion).ToList();
    }

    public async Task<CalificacionItemDto> CreateCalificacionAsync(
        ClaimsPrincipal user,
        CalificacionCreateRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = user.GetRequiredUserId();
        ValidateCalificacionInput(request.Puntuacion, request.RutaId, request.EmpresaId, request.DestinoId);

        await EnsureTargetExistsAsync(request.RutaId, request.EmpresaId, request.DestinoId, ct).ConfigureAwait(false);

        var decision = await _moderation.ModerateUrlAndTextAsync(
            request.Comentario,
            request.ImagenUrl,
            "calificacion",
            ct).ConfigureAwait(false);
        if (!decision.Aprobado)
        {
            throw new RequestValidationException(string.Join(" ", decision.InfraccionesDetectadas));
        }

        var mine = await _store.ListCalificacionesAsync(
            request.RutaId,
            request.EmpresaId,
            request.DestinoId,
            userId,
            ct).ConfigureAwait(false);
        if (mine.Any(c => SameTarget(c, request.RutaId, request.EmpresaId, request.DestinoId)))
        {
            throw new ConflictException("Ya registraste una calificación para este elemento.");
        }

        var account = await _store.GetUsuarioByIdAsync(userId, ct).ConfigureAwait(false);
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = new CalificacionDoc
        {
            Id = $"cal-{Guid.NewGuid():N}",
            UsuarioId = userId,
            UsuarioNombre = account?.NombreCompleto ?? "Pasajero BusTrack",
            RutaId = request.RutaId,
            EmpresaId = request.EmpresaId,
            DestinoId = request.DestinoId?.Trim(),
            Puntuacion = request.Puntuacion,
            Comentario = request.Comentario?.Trim(),
            ImagenUrl = request.ImagenUrl?.Trim(),
            FechaCreacion = now,
            FechaActualizacion = now,
        };

        await _store.UpsertCalificacionAsync(doc, ct).ConfigureAwait(false);
        await RecalculateTargetAverageAsync(doc.RutaId, doc.EmpresaId, doc.DestinoId, ct).ConfigureAwait(false);
        await _catalog.InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        await _push.NotifyUserAsync(
            userId,
            "Opinión publicada",
            "Gracias por ayudar a la comunidad BusTrack con tu calificación.",
            new Dictionary<string, string>(StringComparer.Ordinal) { ["calificacionId"] = doc.Id },
            ct).ConfigureAwait(false);

        return MapCalificacion(doc);
    }

    public async Task<CalificacionItemDto> UpdateCalificacionAsync(
        ClaimsPrincipal user,
        string id,
        CalificacionUpdateRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetCalificacionByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Calificación no encontrada.");

        if (doc.UsuarioId != userId && !user.IsAdmin())
        {
            throw new ForbiddenException("Solo el autor puede editar su calificación.");
        }

        if (request.Puntuacion is < 1 or > 5)
        {
            throw new RequestValidationException("La puntuación debe estar entre 1 y 5 estrellas.");
        }

        var decision = await _moderation.ModerateUrlAndTextAsync(
            request.Comentario,
            request.ImagenUrl,
            "calificacion",
            ct).ConfigureAwait(false);
        if (!decision.Aprobado)
        {
            throw new RequestValidationException(string.Join(" ", decision.InfraccionesDetectadas));
        }

        doc.Puntuacion = request.Puntuacion;
        doc.Comentario = request.Comentario?.Trim();
        doc.ImagenUrl = request.ImagenUrl?.Trim();
        doc.FechaActualizacion = _time.GetUtcNow().UtcDateTime;

        await _store.UpsertCalificacionAsync(doc, ct).ConfigureAwait(false);
        await RecalculateTargetAverageAsync(doc.RutaId, doc.EmpresaId, doc.DestinoId, ct).ConfigureAwait(false);
        await _catalog.InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);

        return MapCalificacion(doc);
    }

    public async Task DeleteCalificacionAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetCalificacionByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Calificación no encontrada.");

        if (doc.UsuarioId != userId && !user.IsAdmin())
        {
            throw new ForbiddenException("No tienes permiso para eliminar esta calificación.");
        }

        await _store.DeleteCalificacionAsync(id, ct).ConfigureAwait(false);
        await RecalculateTargetAverageAsync(doc.RutaId, doc.EmpresaId, doc.DestinoId, ct).ConfigureAwait(false);
        await _catalog.InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ViajeItemDto>> ListViajesAsync(
        ClaimsPrincipal user,
        bool soloFavoritos,
        string? q,
        CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var docs = await _store.ListViajesUsuarioAsync(userId, ct).ConfigureAwait(false);
        var rutas = (await _store.ListRutasAsync(ct).ConfigureAwait(false)).ToDictionary(r => r.Id);

        var filtered = docs.AsEnumerable();
        if (soloFavoritos)
        {
            filtered = filtered.Where(v => v.Favorito);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            filtered = filtered.Where(v =>
                v.Titulo.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (rutas.TryGetValue(v.RutaId, out var r) && r.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        return filtered.Select(v =>
        {
            var rutaNombre = rutas.TryGetValue(v.RutaId, out var r) ? r.Nombre : "Ruta";
            return MapViaje(v, rutaNombre);
        }).ToList();
    }

    public async Task<ViajeDetalleDto> GetViajeDetalleAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetViajeByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Viaje guardado no encontrado.");

        if (doc.UsuarioId != userId && !user.IsAdmin())
        {
            throw new ForbiddenException("El viaje guardado pertenece a otro usuario.");
        }

        var ruta = await _store.GetRutaByIdAsync(doc.RutaId, ct).ConfigureAwait(false);
        return new ViajeDetalleDto(
            doc.Id,
            doc.Titulo,
            doc.RutaId,
            ruta?.Nombre ?? "Ruta",
            ruta?.EmpresaNombre ?? "BusTrack",
            doc.ParadaOrigenId,
            doc.ParadaDestinoId,
            ruta?.TarifaBaseMxn ?? 0m,
            ruta?.Paradas.Count ?? 0,
            doc.Notas,
            doc.Favorito,
            doc.FechaActualizacion);
    }

    public async Task<ViajeItemDto> CreateViajeAsync(
        ClaimsPrincipal user,
        ViajeCreateRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = user.GetRequiredUserId();
        if (string.IsNullOrWhiteSpace(request.Titulo))
        {
            throw new RequestValidationException("El título del viaje es obligatorio.");
        }

        var ruta = await _store.GetRutaByIdAsync(request.RutaId, ct).ConfigureAwait(false)
                   ?? throw new NotFoundException("Ruta no encontrada.");

        var modDecision = await _moderation.ModerateTextAsync(
            $"{request.Titulo} {request.Notas}",
            "viaje",
            ct).ConfigureAwait(false);
        if (!modDecision.Aprobado)
        {
            throw new RequestValidationException(string.Join(" ", modDecision.InfraccionesDetectadas));
        }

        var doc = new ViajeGuardadoDoc
        {
            Id = $"vg-{Guid.NewGuid():N}",
            UsuarioId = userId,
            Titulo = request.Titulo.Trim(),
            RutaId = request.RutaId,
            ParadaOrigenId = request.ParadaOrigenId?.Trim(),
            ParadaDestinoId = request.ParadaDestinoId?.Trim(),
            Notas = request.Notas?.Trim(),
            Favorito = request.Favorito,
            FechaActualizacion = _time.GetUtcNow().UtcDateTime,
        };

        await _store.UpsertViajeAsync(doc, ct).ConfigureAwait(false);
        return MapViaje(doc, ruta.Nombre);
    }

    public async Task<ViajeItemDto> UpdateViajeAsync(
        ClaimsPrincipal user,
        string id,
        ViajeUpdateRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetViajeByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Viaje guardado no encontrado.");

        if (doc.UsuarioId != userId && !user.IsAdmin())
        {
            throw new ForbiddenException("No puedes modificar viajes de otro usuario.");
        }

        if (string.IsNullOrWhiteSpace(request.Titulo))
        {
            throw new RequestValidationException("El título del viaje es obligatorio.");
        }

        var modDecision = await _moderation.ModerateTextAsync(
            $"{request.Titulo} {request.Notas}",
            "viaje",
            ct).ConfigureAwait(false);
        if (!modDecision.Aprobado)
        {
            throw new RequestValidationException(string.Join(" ", modDecision.InfraccionesDetectadas));
        }

        doc.Titulo = request.Titulo.Trim();
        doc.ParadaOrigenId = request.ParadaOrigenId?.Trim();
        doc.ParadaDestinoId = request.ParadaDestinoId?.Trim();
        doc.Notas = request.Notas?.Trim();
        doc.Favorito = request.Favorito;
        doc.FechaActualizacion = _time.GetUtcNow().UtcDateTime;

        await _store.UpsertViajeAsync(doc, ct).ConfigureAwait(false);
        var ruta = await _store.GetRutaByIdAsync(doc.RutaId, ct).ConfigureAwait(false);
        return MapViaje(doc, ruta?.Nombre ?? "Ruta");
    }

    public async Task DeleteViajeAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = user.GetRequiredUserId();
        var doc = await _store.GetViajeByIdAsync(id, ct).ConfigureAwait(false)
                  ?? throw new NotFoundException("Viaje guardado no encontrado.");

        if (doc.UsuarioId != userId && !user.IsAdmin())
        {
            throw new ForbiddenException("No puedes eliminar viajes de otro usuario.");
        }

        await _store.DeleteViajeAsync(id, ct).ConfigureAwait(false);
    }

    private static void ValidateCalificacionInput(int puntuacion, Guid? rutaId, Guid? empresaId, string? destinoId)
    {
        if (puntuacion is < 1 or > 5)
        {
            throw new RequestValidationException("La puntuación debe estar entre 1 y 5 estrellas.");
        }

        if (!rutaId.HasValue && !empresaId.HasValue && string.IsNullOrWhiteSpace(destinoId))
        {
            throw new RequestValidationException("Debes indicar rutaId, empresaId o destinoId a calificar.");
        }
    }

    private async Task EnsureTargetExistsAsync(Guid? rutaId, Guid? empresaId, string? destinoId, CancellationToken ct)
    {
        if (rutaId.HasValue && await _store.GetRutaByIdAsync(rutaId.Value, ct).ConfigureAwait(false) is null)
        {
            throw new NotFoundException("La ruta a calificar no existe.");
        }

        if (empresaId.HasValue && await _store.GetEmpresaByIdAsync(empresaId.Value, ct).ConfigureAwait(false) is null)
        {
            throw new NotFoundException("La empresa a calificar no existe.");
        }

        if (!string.IsNullOrWhiteSpace(destinoId)
            && await _store.GetDestinoByIdAsync(destinoId.Trim(), ct).ConfigureAwait(false) is null)
        {
            throw new NotFoundException("El destino turístico a calificar no existe.");
        }
    }

    private async Task RecalculateTargetAverageAsync(Guid? rutaId, Guid? empresaId, string? destinoId, CancellationToken ct)
    {
        if (rutaId.HasValue)
        {
            var ruta = await _store.GetRutaByIdAsync(rutaId.Value, ct).ConfigureAwait(false);
            if (ruta is not null)
            {
                var reviews = await _store.ListCalificacionesAsync(rutaId.Value, null, null, null, ct).ConfigureAwait(false);
                ruta.CalificacionPromedio = reviews.Count > 0 ? Math.Round(reviews.Average(x => x.Puntuacion), 2) : 4.5;
                await _store.UpsertRutaAsync(ruta, ct).ConfigureAwait(false);
            }
        }

        if (empresaId.HasValue)
        {
            var empresa = await _store.GetEmpresaByIdAsync(empresaId.Value, ct).ConfigureAwait(false);
            if (empresa is not null)
            {
                var reviews = await _store.ListCalificacionesAsync(null, empresaId.Value, null, null, ct).ConfigureAwait(false);
                empresa.TotalEvaluaciones = reviews.Count;
                empresa.CalificacionPromedio = reviews.Count > 0 ? Math.Round(reviews.Average(x => x.Puntuacion), 2) : 4.5;
                await _store.UpsertEmpresaAsync(empresa, ct).ConfigureAwait(false);
            }
        }

        if (!string.IsNullOrWhiteSpace(destinoId))
        {
            var destino = await _store.GetDestinoByIdAsync(destinoId, ct).ConfigureAwait(false);
            if (destino is not null)
            {
                var reviews = await _store.ListCalificacionesAsync(null, null, destinoId, null, ct).ConfigureAwait(false);
                destino.CalificacionPromedio = reviews.Count > 0 ? Math.Round(reviews.Average(x => x.Puntuacion), 2) : 4.6;
                await _store.UpsertDestinoAsync(destino, ct).ConfigureAwait(false);
            }
        }
    }

    private static bool SameTarget(CalificacionDoc doc, Guid? rutaId, Guid? empresaId, string? destinoId) =>
        doc.RutaId == rutaId
        && doc.EmpresaId == empresaId
        && string.Equals(doc.DestinoId, destinoId?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static CalificacionItemDto MapCalificacion(CalificacionDoc d) =>
        new(
            d.Id,
            d.UsuarioId,
            d.UsuarioNombre,
            d.RutaId,
            d.EmpresaId,
            d.DestinoId,
            d.Puntuacion,
            d.Comentario,
            d.ImagenUrl,
            d.FechaCreacion,
            d.FechaActualizacion);

    private static ViajeItemDto MapViaje(ViajeGuardadoDoc d, string rutaNombre) =>
        new(
            d.Id,
            d.Titulo,
            d.RutaId,
            rutaNombre,
            d.ParadaOrigenId,
            d.ParadaDestinoId,
            d.Notas,
            d.Favorito,
            d.FechaActualizacion);
}
