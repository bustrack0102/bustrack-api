using System.Collections.Concurrent;

namespace ApiSDD.Data.NoSql;

/// <summary>
/// Implementación en memoria de IBusTrackNoSqlStore precargada con BusTrackSeedData.
/// Habilita pruebas de integración, contrato y seguridad 100% deterministas sin Docker ni red.
/// </summary>
internal sealed class InMemoryBusTrackStore : IBusTrackNoSqlStore
{
    private readonly ConcurrentDictionary<Guid, UsuarioDoc> _usuarios = new();
    private readonly ConcurrentDictionary<Guid, RefreshTokenDoc> _refreshTokens = new();
    private readonly ConcurrentDictionary<Guid, TokenRecuperacionDoc> _recoveryTokens = new();
    private readonly ConcurrentDictionary<string, BloqueoLoginDoc> _locks = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<int, MedioTransporteDoc> _medios = new();
    private readonly ConcurrentDictionary<Guid, EmpresaDoc> _empresas = new();
    private readonly ConcurrentDictionary<string, ParadaDoc> _paradas = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, RutaDoc> _rutas = new();
    private readonly ConcurrentDictionary<string, HorarioDoc> _horarios = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TarifaDoc> _tarifas = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DestinoDoc> _destinos = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, CalificacionDoc> _calificaciones = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ViajeGuardadoDoc> _viajes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, ConsentimientoSensorDoc> _consentimientos = new();
    private readonly ConcurrentDictionary<string, LoteSensorDoc> _lotes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CandidatoParadaDoc> _candidatos = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SolicitudRecomendacionDoc> _recomendaciones = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, FuenteDatoDoc> _fuentes = new();
    private readonly ConcurrentDictionary<string, ImportacionDoc> _importaciones = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DispositivoFcmDoc> _dispositivos = new(StringComparer.OrdinalIgnoreCase);

    private int _medioSeq = 10;

    public InMemoryBusTrackStore()
    {
        foreach (var m in BusTrackSeedData.MediosTransporte()) _medios[m.Id] = m;
        foreach (var e in BusTrackSeedData.Empresas()) _empresas[e.Id] = e;
        foreach (var p in BusTrackSeedData.Paradas()) _paradas[p.Id] = p;
        foreach (var r in BusTrackSeedData.Rutas()) _rutas[r.Id] = r;
        foreach (var h in BusTrackSeedData.Horarios()) _horarios[h.Id] = h;
        foreach (var t in BusTrackSeedData.Tarifas()) _tarifas[t.Id] = t;
        foreach (var d in BusTrackSeedData.Destinos()) _destinos[d.Id] = d;
        foreach (var f in BusTrackSeedData.FuentesDatos()) _fuentes[f.Id] = f;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
    }

    public DateTime CatalogUpdatedAtUtc { get; private set; }

    public Task EnsureRoleSeededAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<UsuarioDoc?> GetUsuarioByEmailAsync(string emailNormalized, CancellationToken ct) =>
        Task.FromResult(_usuarios.Values.FirstOrDefault(u => string.Equals(u.Correo, emailNormalized, StringComparison.OrdinalIgnoreCase)));

    public Task<UsuarioDoc?> GetUsuarioByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_usuarios.GetValueOrDefault(id));

    public Task<bool> ExistsUsuarioByEmailAsync(string emailNormalized, Guid? excludeId, CancellationToken ct) =>
        Task.FromResult(_usuarios.Values.Any(u =>
            string.Equals(u.Correo, emailNormalized, StringComparison.OrdinalIgnoreCase)
            && (!excludeId.HasValue || u.Id != excludeId.Value)));

    public Task<(IReadOnlyList<UsuarioDoc> Items, int Total)> ListUsuariosAdminAsync(
        string? q, string? rol, bool? activo, int page, int pageSize, string? sortBy, string? dir, CancellationToken ct)
    {
        var query = _usuarios.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u =>
                u.NombreCompleto.Contains(term, StringComparison.OrdinalIgnoreCase)
                || u.Correo.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(rol))
        {
            query = query.Where(u => string.Equals(u.RolNombre, rol.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (activo.HasValue)
        {
            query = query.Where(u => u.Activo == activo.Value);
        }

        var desc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy ?? "fechaCreacion").ToLowerInvariant() switch
        {
            "nombrecompleto" => desc ? query.OrderByDescending(u => u.NombreCompleto) : query.OrderBy(u => u.NombreCompleto),
            "correo" => desc ? query.OrderByDescending(u => u.Correo) : query.OrderBy(u => u.Correo),
            _ => desc ? query.OrderByDescending(u => u.FechaCreacion) : query.OrderBy(u => u.FechaCreacion),
        };

        var materialized = query.ToList();
        var items = materialized.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<UsuarioDoc>, int)>((items, materialized.Count));
    }

    public Task CreateUsuarioAsync(UsuarioDoc usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        _ = _usuarios.TryAdd(usuario.Id, usuario);
        return Task.CompletedTask;
    }

    public Task UpdateUsuarioAsync(UsuarioDoc usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        _usuarios[usuario.Id] = usuario;
        return Task.CompletedTask;
    }

    public Task<RefreshTokenDoc?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct) =>
        Task.FromResult(_refreshTokens.Values.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task CreateRefreshTokenAsync(RefreshTokenDoc token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);
        _ = _refreshTokens.TryAdd(token.Id, token);
        return Task.CompletedTask;
    }

    public Task UpdateRefreshTokenAsync(RefreshTokenDoc token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);
        _refreshTokens[token.Id] = token;
        return Task.CompletedTask;
    }

    public Task RevokeAllRefreshTokensForUserAsync(Guid usuarioId, DateTime revokedAtUtc, CancellationToken ct)
    {
        foreach (var rt in _refreshTokens.Values.Where(t => t.UsuarioId == usuarioId && !t.Revocado))
        {
            rt.Revocado = true;
            rt.FechaRevocacion = revokedAtUtc;
        }

        return Task.CompletedTask;
    }

    public Task<TokenRecuperacionDoc?> GetRecoveryTokenByHashAsync(string tokenHash, CancellationToken ct) =>
        Task.FromResult(_recoveryTokens.Values.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task InvalidateActiveRecoveryTokensAsync(Guid usuarioId, CancellationToken ct)
    {
        foreach (var t in _recoveryTokens.Values.Where(x => x.UsuarioId == usuarioId && !x.Usado))
        {
            t.Usado = true;
        }

        return Task.CompletedTask;
    }

    public Task CreateRecoveryTokenAsync(TokenRecuperacionDoc token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);
        _ = _recoveryTokens.TryAdd(token.Id, token);
        return Task.CompletedTask;
    }

    public Task UpdateRecoveryTokenAsync(TokenRecuperacionDoc token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);
        _recoveryTokens[token.Id] = token;
        return Task.CompletedTask;
    }

    public Task<BloqueoLoginDoc?> GetLoginLockAsync(string key, CancellationToken ct) =>
        Task.FromResult(_locks.GetValueOrDefault(key));

    public Task UpsertLoginLockAsync(BloqueoLoginDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _locks[doc.NombreUsuario] = doc;
        return Task.CompletedTask;
    }

    public Task DeleteLoginLockAsync(string key, CancellationToken ct)
    {
        _locks.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EmpresaDoc>> ListEmpresasAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<EmpresaDoc>>(_empresas.Values.ToList());

    public Task<EmpresaDoc?> GetEmpresaByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_empresas.GetValueOrDefault(id));

    public Task UpsertEmpresaAsync(EmpresaDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _empresas[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MedioTransporteDoc>> ListMediosTransporteAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<MedioTransporteDoc>>(_medios.Values.ToList());

    public Task<MedioTransporteDoc?> GetMedioByIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_medios.GetValueOrDefault(id));

    public Task<MedioTransporteDoc> CreateMedioTransporteAsync(MedioTransporteDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        if (doc.Id <= 0)
        {
            doc.Id = Interlocked.Increment(ref _medioSeq);
        }

        _medios[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.FromResult(doc);
    }

    public Task UpdateMedioTransporteAsync(MedioTransporteDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _medios[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RutaDoc>> ListRutasAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<RutaDoc>>(_rutas.Values.ToList());

    public Task<RutaDoc?> GetRutaByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_rutas.GetValueOrDefault(id));

    public Task UpsertRutaAsync(RutaDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _rutas[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ParadaDoc>> ListParadasAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ParadaDoc>>(_paradas.Values.ToList());

    public Task<ParadaDoc?> GetParadaByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_paradas.GetValueOrDefault(id));

    public Task<IReadOnlyList<(ParadaDoc Parada, double DistanciaKm)>> FindParadasNearAsync(double lat, double lon, double radioKm, CancellationToken ct)
    {
        var list = _paradas.Values
            .Where(p => p.Activo)
            .Select(p => (Parada: p, DistanciaKm: GeoMath.DistanceKm(lat, lon, p.Ubicacion.Latitud, p.Ubicacion.Longitud)))
            .Where(x => x.DistanciaKm <= radioKm)
            .OrderBy(x => x.DistanciaKm)
            .ToList();
        return Task.FromResult<IReadOnlyList<(ParadaDoc, double)>>(list);
    }

    public Task UpsertParadaAsync(ParadaDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _paradas[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<HorarioDoc>> ListHorariosAsync(Guid? rutaId, CancellationToken ct)
    {
        var q = _horarios.Values.AsEnumerable();
        if (rutaId.HasValue) q = q.Where(h => h.RutaId == rutaId.Value);
        return Task.FromResult<IReadOnlyList<HorarioDoc>>(q.ToList());
    }

    public Task UpsertHorarioAsync(HorarioDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _horarios[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<HorarioDoc?> GetHorarioByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_horarios.GetValueOrDefault(id));

    public Task<IReadOnlyList<TarifaDoc>> ListTarifasAsync(Guid? rutaId, CancellationToken ct)
    {
        var q = _tarifas.Values.AsEnumerable();
        if (rutaId.HasValue) q = q.Where(t => t.RutaId == rutaId.Value);
        return Task.FromResult<IReadOnlyList<TarifaDoc>>(q.ToList());
    }

    public Task UpsertTarifaAsync(TarifaDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _tarifas[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<TarifaDoc?> GetTarifaByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_tarifas.GetValueOrDefault(id));

    public Task<IReadOnlyList<DestinoDoc>> ListDestinosAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DestinoDoc>>(_destinos.Values.ToList());

    public Task<DestinoDoc?> GetDestinoByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_destinos.GetValueOrDefault(id));

    public Task UpsertDestinoAsync(DestinoDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _destinos[doc.Id] = doc;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CalificacionDoc>> ListCalificacionesAsync(Guid? rutaId, Guid? empresaId, string? destinoId, Guid? usuarioId, CancellationToken ct)
    {
        var q = _calificaciones.Values.AsEnumerable();
        if (rutaId.HasValue) q = q.Where(c => c.RutaId == rutaId.Value);
        if (empresaId.HasValue) q = q.Where(c => c.EmpresaId == empresaId.Value);
        if (!string.IsNullOrWhiteSpace(destinoId)) q = q.Where(c => string.Equals(c.DestinoId, destinoId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (usuarioId.HasValue) q = q.Where(c => c.UsuarioId == usuarioId.Value);
        return Task.FromResult<IReadOnlyList<CalificacionDoc>>(q.OrderByDescending(c => c.FechaCreacion).ToList());
    }

    public Task<CalificacionDoc?> GetCalificacionByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_calificaciones.GetValueOrDefault(id));

    public Task UpsertCalificacionAsync(CalificacionDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _calificaciones[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task DeleteCalificacionAsync(string id, CancellationToken ct)
    {
        _calificaciones.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ViajeGuardadoDoc>> ListViajesUsuarioAsync(Guid usuarioId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ViajeGuardadoDoc>>(
            _viajes.Values.Where(v => v.UsuarioId == usuarioId).OrderByDescending(v => v.FechaActualizacion).ToList());

    public Task<ViajeGuardadoDoc?> GetViajeByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_viajes.GetValueOrDefault(id));

    public Task UpsertViajeAsync(ViajeGuardadoDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _viajes[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task DeleteViajeAsync(string id, CancellationToken ct)
    {
        _viajes.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<ConsentimientoSensorDoc?> GetConsentimientoAsync(Guid usuarioId, CancellationToken ct) =>
        Task.FromResult(_consentimientos.GetValueOrDefault(usuarioId));

    public Task UpsertConsentimientoAsync(ConsentimientoSensorDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _consentimientos[doc.UsuarioId] = doc;
        return Task.CompletedTask;
    }

    public Task CreateLoteSensorAsync(LoteSensorDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _lotes[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LoteSensorDoc>> ListLotesSensorAsync(Guid? rutaId, CancellationToken ct)
    {
        var q = _lotes.Values.AsEnumerable();
        if (rutaId.HasValue) q = q.Where(x => x.RutaId == rutaId.Value);
        return Task.FromResult<IReadOnlyList<LoteSensorDoc>>(q.OrderByDescending(x => x.CreadoEnUtc).ToList());
    }

    public Task CreateCandidatoParadaAsync(CandidatoParadaDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _ = _candidatos.TryAdd(doc.Id, doc);
        return Task.CompletedTask;
    }

    public Task<CandidatoParadaDoc?> GetCandidatoParadaByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_candidatos.GetValueOrDefault(id));

    public Task<IReadOnlyList<CandidatoParadaDoc>> ListCandidatosParadaAsync(string? estado, Guid? rutaId, CancellationToken ct)
    {
        var q = _candidatos.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => string.Equals(x.Estado, estado.Trim(), StringComparison.OrdinalIgnoreCase));
        if (rutaId.HasValue) q = q.Where(x => x.RutaId == rutaId.Value);
        return Task.FromResult<IReadOnlyList<CandidatoParadaDoc>>(q.OrderByDescending(x => x.CreadoEnUtc).ToList());
    }

    public Task UpdateCandidatoParadaAsync(CandidatoParadaDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _candidatos[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task CreateRecomendacionAsync(SolicitudRecomendacionDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _ = _recomendaciones.TryAdd(doc.Id, doc);
        return Task.CompletedTask;
    }

    public Task<SolicitudRecomendacionDoc?> GetRecomendacionByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_recomendaciones.GetValueOrDefault(id));

    public Task<IReadOnlyList<SolicitudRecomendacionDoc>> ListRecomendacionesAsync(Guid? usuarioId, CancellationToken ct)
    {
        var q = _recomendaciones.Values.AsEnumerable();
        if (usuarioId.HasValue) q = q.Where(x => x.UsuarioId == usuarioId.Value);
        return Task.FromResult<IReadOnlyList<SolicitudRecomendacionDoc>>(q.OrderByDescending(x => x.CreadoEnUtc).ToList());
    }

    public Task UpdateRecomendacionAsync(SolicitudRecomendacionDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _recomendaciones[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<FuenteDatoDoc>> ListFuentesAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<FuenteDatoDoc>>(_fuentes.Values.ToList());

    public Task<FuenteDatoDoc?> GetFuenteByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_fuentes.GetValueOrDefault(id));

    public Task UpsertFuenteAsync(FuenteDatoDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _fuentes[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task CreateImportacionAsync(ImportacionDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _importaciones[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ImportacionDoc>> ListImportacionesAsync(Guid? fuenteId, CancellationToken ct)
    {
        var q = _importaciones.Values.AsEnumerable();
        if (fuenteId.HasValue) q = q.Where(x => x.FuenteDatoId == fuenteId.Value);
        return Task.FromResult<IReadOnlyList<ImportacionDoc>>(q.OrderByDescending(x => x.EjecutadoEnUtc).ToList());
    }

    public Task UpsertDispositivoFcmAsync(DispositivoFcmDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        _dispositivos[doc.Id] = doc;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DispositivoFcmDoc>> ListDispositivosUsuarioAsync(Guid usuarioId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DispositivoFcmDoc>>(_dispositivos.Values.Where(x => x.UsuarioId == usuarioId).ToList());

    public Task<bool> DeleteDispositivoFcmAsync(string id, Guid usuarioId, CancellationToken ct)
    {
        if (_dispositivos.TryGetValue(id, out var existing) && existing.UsuarioId == usuarioId)
        {
            return Task.FromResult(_dispositivos.TryRemove(id, out _));
        }

        return Task.FromResult(false);
    }
}
