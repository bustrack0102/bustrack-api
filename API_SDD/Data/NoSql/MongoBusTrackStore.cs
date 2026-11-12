using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace ApiSDD.Data.NoSql;

/// <summary>
/// Implementación MongoDB Atlas de IBusTrackNoSqlStore con consultas BSON y geoespaciales ($nearSphere).
/// </summary>
internal sealed partial class MongoBusTrackStore : IBusTrackNoSqlStore, IDisposable
{
    private readonly MongoDbContext _mongo;
    private readonly ILogger<MongoBusTrackStore> _logger;
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    private volatile bool _seeded;

    public MongoBusTrackStore(MongoDbContext mongo, ILogger<MongoBusTrackStore>? logger = null)
    {
        _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
        _logger = logger ?? NullLogger<MongoBusTrackStore>.Instance;
        CatalogUpdatedAtUtc = DateTime.UtcNow;
    }

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Omitiendo documento heredado incompatible en colección '{CollectionName}'.")]
    private static partial void LogLegacyDocSkipped(ILogger logger, string collectionName, Exception ex);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Consulta geoespacial $nearSphere en 'paradas' sin índice activo; aplicando filtrado Haversine de respaldo.")]
    private static partial void LogGeoNearFallback(ILogger logger, Exception ex);

    public void Dispose() => _seedLock.Dispose();

    public DateTime CatalogUpdatedAtUtc { get; private set; }

    private IMongoCollection<RolDoc> Roles => _mongo.GetCollection<RolDoc>("roles");
    private IMongoCollection<UsuarioDoc> Usuarios => _mongo.GetCollection<UsuarioDoc>("usuarios");
    private IMongoCollection<RefreshTokenDoc> RefreshTokens => _mongo.GetCollection<RefreshTokenDoc>("refresh_tokens");
    private IMongoCollection<TokenRecuperacionDoc> RecoveryTokens => _mongo.GetCollection<TokenRecuperacionDoc>("tokens_recuperacion");
    private IMongoCollection<BloqueoLoginDoc> Locks => _mongo.GetCollection<BloqueoLoginDoc>("seg_bloqueo");
    private IMongoCollection<MedioTransporteDoc> Medios => _mongo.GetCollection<MedioTransporteDoc>("mediosTransporte");
    private IMongoCollection<EmpresaDoc> Empresas => _mongo.GetCollection<EmpresaDoc>("empresas");
    private IMongoCollection<ParadaDoc> Paradas => _mongo.GetCollection<ParadaDoc>("paradas");
    private IMongoCollection<RutaDoc> Rutas => _mongo.GetCollection<RutaDoc>("rutas");
    private IMongoCollection<HorarioDoc> Horarios => _mongo.GetCollection<HorarioDoc>("horarios");
    private IMongoCollection<TarifaDoc> Tarifas => _mongo.GetCollection<TarifaDoc>("tarifas");
    private IMongoCollection<DestinoDoc> Destinos => _mongo.GetCollection<DestinoDoc>("destinos");
    private IMongoCollection<CalificacionDoc> Calificaciones => _mongo.GetCollection<CalificacionDoc>("calificaciones");
    private IMongoCollection<ViajeGuardadoDoc> Viajes => _mongo.GetCollection<ViajeGuardadoDoc>("viajes_guardados");
    private IMongoCollection<ConsentimientoSensorDoc> Consentimientos => _mongo.GetCollection<ConsentimientoSensorDoc>("consentimientos_sensores");
    private IMongoCollection<LoteSensorDoc> Lotes => _mongo.GetCollection<LoteSensorDoc>("lotes_sensores");
    private IMongoCollection<CandidatoParadaDoc> Candidatos => _mongo.GetCollection<CandidatoParadaDoc>("candidatos_parada");
    private IMongoCollection<SolicitudRecomendacionDoc> Recomendaciones => _mongo.GetCollection<SolicitudRecomendacionDoc>("solicitudes_recomendacion");
    private IMongoCollection<FuenteDatoDoc> Fuentes => _mongo.GetCollection<FuenteDatoDoc>("fuentes_datos");
    private IMongoCollection<ImportacionDoc> Importaciones => _mongo.GetCollection<ImportacionDoc>("importaciones");
    private IMongoCollection<DispositivoFcmDoc> Dispositivos => _mongo.GetCollection<DispositivoFcmDoc>("dispositivos_fcm");

    public async Task EnsureRoleSeededAsync(CancellationToken ct)
    {
        if (_seeded || !_mongo.IsConfigured)
        {
            return;
        }

        await _seedLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_seeded)
            {
                return;
            }

            await EnsureSeedCollectionAsync(
                Roles,
                Builders<RolDoc>.Filter.Eq(x => x.Nombre, "Usuario"),
                BusTrackSeedData.Roles(),
                seed => Builders<RolDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Medios,
                Builders<MedioTransporteDoc>.Filter.Eq(x => x.Clave, "BUS-DIR"),
                BusTrackSeedData.MediosTransporte(),
                seed => Builders<MedioTransporteDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Empresas,
                Builders<EmpresaDoc>.Filter.Eq(x => x.Id, BusTrackSeedData.EmpresaAvmId),
                BusTrackSeedData.Empresas(),
                seed => Builders<EmpresaDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Paradas,
                Builders<ParadaDoc>.Filter.Eq(x => x.Id, "PAR-01"),
                BusTrackSeedData.Paradas(),
                seed => Builders<ParadaDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Rutas,
                Builders<RutaDoc>.Filter.Eq(x => x.Id, BusTrackSeedData.Ruta1Id),
                BusTrackSeedData.Rutas(),
                seed => Builders<RutaDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Horarios,
                Builders<HorarioDoc>.Filter.Eq(x => x.Id, "HOR-01"),
                BusTrackSeedData.Horarios(),
                seed => Builders<HorarioDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Tarifas,
                Builders<TarifaDoc>.Filter.Eq(x => x.Id, "TAR-01"),
                BusTrackSeedData.Tarifas(),
                seed => Builders<TarifaDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Destinos,
                Builders<DestinoDoc>.Filter.Eq(x => x.Id, "DES-01"),
                BusTrackSeedData.Destinos(),
                seed => Builders<DestinoDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            await EnsureSeedCollectionAsync(
                Fuentes,
                Builders<FuenteDatoDoc>.Filter.Eq(x => x.Id, BusTrackSeedData.FuenteOficialId),
                BusTrackSeedData.FuentesDatos(),
                seed => Builders<FuenteDatoDoc>.Filter.Eq(x => x.Id, seed.Id),
                ct).ConfigureAwait(false);

            _seeded = true;
        }
        finally
        {
            _seedLock.Release();
        }
    }

    private static async Task EnsureSeedCollectionAsync<TDoc>(
        IMongoCollection<TDoc> collection,
        FilterDefinition<TDoc> probeFilter,
        IReadOnlyList<TDoc> seeds,
        Func<TDoc, FilterDefinition<TDoc>> idFilter,
        CancellationToken ct)
    {
        if (seeds.Count == 0
            || await collection.CountDocumentsAsync(probeFilter, cancellationToken: ct).ConfigureAwait(false) > 0)
        {
            return;
        }

        var writes = seeds
            .Select(seed => (WriteModel<TDoc>)new ReplaceOneModel<TDoc>(idFilter(seed), seed) { IsUpsert = true })
            .ToList();
        await collection.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false }, ct).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<TDoc>> ListCompatibleAsync<TDoc>(
        string collectionName,
        FilterDefinition<BsonDocument>? filter,
        CancellationToken ct)
    {
        var raw = await _mongo.RawCollection(collectionName)
            .Find(filter ?? Builders<BsonDocument>.Filter.Empty)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return DeserializeCompatible<TDoc>(raw, collectionName, _logger);
    }

    internal static IReadOnlyList<TDoc> DeserializeCompatible<TDoc>(
        IEnumerable<BsonDocument> rawDocs,
        string collectionName,
        ILogger? logger = null)
    {
        var items = new List<TDoc>();
        foreach (var rawDoc in rawDocs)
        {
            try
            {
                items.Add(BsonSerializer.Deserialize<TDoc>(rawDoc));
            }
            catch (Exception ex) when (ex is FormatException or BsonSerializationException)
            {
                if (logger is not null)
                {
                    LogLegacyDocSkipped(logger, collectionName, ex);
                }
            }
        }

        return items;
    }

    public async Task<UsuarioDoc?> GetUsuarioByEmailAsync(string emailNormalized, CancellationToken ct) =>
        await Usuarios.Find(u => u.Correo == emailNormalized).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<UsuarioDoc?> GetUsuarioByIdAsync(Guid id, CancellationToken ct) =>
        await Usuarios.Find(u => u.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<bool> ExistsUsuarioByEmailAsync(string emailNormalized, Guid? excludeId, CancellationToken ct)
    {
        var filter = excludeId.HasValue
            ? Builders<UsuarioDoc>.Filter.And(
                Builders<UsuarioDoc>.Filter.Eq(u => u.Correo, emailNormalized),
                Builders<UsuarioDoc>.Filter.Ne(u => u.Id, excludeId.Value))
            : Builders<UsuarioDoc>.Filter.Eq(u => u.Correo, emailNormalized);
        return await Usuarios.Find(filter).AnyAsync(ct).ConfigureAwait(false);
    }

    public async Task<(IReadOnlyList<UsuarioDoc> Items, int Total)> ListUsuariosAdminAsync(
        string? q, string? rol, bool? activo, int page, int pageSize, string? sortBy, string? dir, CancellationToken ct)
    {
        var fb = Builders<UsuarioDoc>.Filter;
        var filters = new List<FilterDefinition<UsuarioDoc>>();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(q.Trim()), "i");
            filters.Add(fb.Or(fb.Regex(u => u.NombreCompleto, regex), fb.Regex(u => u.Correo, regex)));
        }

        if (!string.IsNullOrWhiteSpace(rol))
        {
            filters.Add(fb.Eq(u => u.RolNombre, rol.Trim()));
        }

        if (activo.HasValue)
        {
            filters.Add(fb.Eq(u => u.Activo, activo.Value));
        }

        var combined = filters.Count == 0 ? fb.Empty : fb.And(filters);
        var total = (int)await Usuarios.CountDocumentsAsync(combined, cancellationToken: ct).ConfigureAwait(false);

        var desc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
        var sort = (sortBy ?? "fechaCreacion").ToLowerInvariant() switch
        {
            "nombrecompleto" => desc ? Builders<UsuarioDoc>.Sort.Descending(u => u.NombreCompleto) : Builders<UsuarioDoc>.Sort.Ascending(u => u.NombreCompleto),
            "correo" => desc ? Builders<UsuarioDoc>.Sort.Descending(u => u.Correo) : Builders<UsuarioDoc>.Sort.Ascending(u => u.Correo),
            _ => desc ? Builders<UsuarioDoc>.Sort.Descending(u => u.FechaCreacion) : Builders<UsuarioDoc>.Sort.Ascending(u => u.FechaCreacion),
        };

        var items = await Usuarios.Find(combined).Sort(sort).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }

    public Task CreateUsuarioAsync(UsuarioDoc usuario, CancellationToken ct) =>
        Usuarios.InsertOneAsync(usuario, cancellationToken: ct);

    public Task UpdateUsuarioAsync(UsuarioDoc usuario, CancellationToken ct) =>
        Usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario, new ReplaceOptions { IsUpsert = true }, ct);

    public async Task<RefreshTokenDoc?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct) =>
        await RefreshTokens.Find(t => t.TokenHash == tokenHash).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task CreateRefreshTokenAsync(RefreshTokenDoc token, CancellationToken ct) =>
        RefreshTokens.InsertOneAsync(token, cancellationToken: ct);

    public Task UpdateRefreshTokenAsync(RefreshTokenDoc token, CancellationToken ct) =>
        RefreshTokens.ReplaceOneAsync(t => t.Id == token.Id, token, new ReplaceOptions { IsUpsert = true }, ct);

    public Task RevokeAllRefreshTokensForUserAsync(Guid usuarioId, DateTime revokedAtUtc, CancellationToken ct)
    {
        var filter = Builders<RefreshTokenDoc>.Filter.And(
            Builders<RefreshTokenDoc>.Filter.Eq(t => t.UsuarioId, usuarioId),
            Builders<RefreshTokenDoc>.Filter.Eq(t => t.Revocado, false));
        var update = Builders<RefreshTokenDoc>.Update
            .Set(t => t.Revocado, true)
            .Set(t => t.FechaRevocacion, revokedAtUtc);
        return RefreshTokens.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public async Task<TokenRecuperacionDoc?> GetRecoveryTokenByHashAsync(string tokenHash, CancellationToken ct) =>
        await RecoveryTokens.Find(t => t.TokenHash == tokenHash).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task InvalidateActiveRecoveryTokensAsync(Guid usuarioId, CancellationToken ct)
    {
        var filter = Builders<TokenRecuperacionDoc>.Filter.And(
            Builders<TokenRecuperacionDoc>.Filter.Eq(t => t.UsuarioId, usuarioId),
            Builders<TokenRecuperacionDoc>.Filter.Eq(t => t.Usado, false));
        var update = Builders<TokenRecuperacionDoc>.Update.Set(t => t.Usado, true);
        return RecoveryTokens.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public Task CreateRecoveryTokenAsync(TokenRecuperacionDoc token, CancellationToken ct) =>
        RecoveryTokens.InsertOneAsync(token, cancellationToken: ct);

    public Task UpdateRecoveryTokenAsync(TokenRecuperacionDoc token, CancellationToken ct) =>
        RecoveryTokens.ReplaceOneAsync(t => t.Id == token.Id, token, new ReplaceOptions { IsUpsert = true }, ct);

    public async Task<BloqueoLoginDoc?> GetLoginLockAsync(string key, CancellationToken ct) =>
        await Locks.Find(b => b.NombreUsuario == key).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task UpsertLoginLockAsync(BloqueoLoginDoc doc, CancellationToken ct) =>
        Locks.ReplaceOneAsync(b => b.NombreUsuario == doc.NombreUsuario, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public Task DeleteLoginLockAsync(string key, CancellationToken ct) =>
        Locks.DeleteOneAsync(b => b.NombreUsuario == key, ct);

    public async Task<IReadOnlyList<EmpresaDoc>> ListEmpresasAsync(CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await ListCompatibleAsync<EmpresaDoc>("empresas", null, ct).ConfigureAwait(false);
    }

    public async Task<EmpresaDoc?> GetEmpresaByIdAsync(Guid id, CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await Empresas.Find(e => e.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }

    public Task UpsertEmpresaAsync(EmpresaDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Empresas.ReplaceOneAsync(e => e.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<IReadOnlyList<MedioTransporteDoc>> ListMediosTransporteAsync(CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await ListCompatibleAsync<MedioTransporteDoc>("mediosTransporte", null, ct).ConfigureAwait(false);
    }

    public async Task<MedioTransporteDoc?> GetMedioByIdAsync(int id, CancellationToken ct) =>
        await Medios.Find(m => m.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<MedioTransporteDoc> CreateMedioTransporteAsync(MedioTransporteDoc doc, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(doc);
        if (doc.Id <= 0)
        {
            var last = await Medios.Find(Builders<MedioTransporteDoc>.Filter.Empty)
                .SortByDescending(m => m.Id).FirstOrDefaultAsync(ct).ConfigureAwait(false);
            doc.Id = (last?.Id ?? 0) + 1;
        }

        await Medios.InsertOneAsync(doc, cancellationToken: ct).ConfigureAwait(false);
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return doc;
    }

    public Task UpdateMedioTransporteAsync(MedioTransporteDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Medios.ReplaceOneAsync(m => m.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<IReadOnlyList<RutaDoc>> ListRutasAsync(CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await ListCompatibleAsync<RutaDoc>("rutas", null, ct).ConfigureAwait(false);
    }

    public async Task<RutaDoc?> GetRutaByIdAsync(Guid id, CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await Rutas.Find(r => r.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }

    public Task UpsertRutaAsync(RutaDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Rutas.ReplaceOneAsync(r => r.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<IReadOnlyList<ParadaDoc>> ListParadasAsync(CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await Paradas.Find(Builders<ParadaDoc>.Filter.Empty).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<ParadaDoc?> GetParadaByIdAsync(string id, CancellationToken ct) =>
        await Paradas.Find(p => p.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<(ParadaDoc Parada, double DistanciaKm)>> FindParadasNearAsync(double lat, double lon, double radioKm, CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        IReadOnlyList<ParadaDoc> hits;
        try
        {
            var point = GeoJson.Point(GeoJson.Geographic(lon, lat));
            var filter = Builders<ParadaDoc>.Filter.And(
                Builders<ParadaDoc>.Filter.Eq(p => p.Activo, true),
                Builders<ParadaDoc>.Filter.NearSphere("ubicacion", point, radioKm * 1000.0));
            hits = await Paradas.Find(filter).ToListAsync(ct).ConfigureAwait(false);
        }
        catch (MongoCommandException ex) when (ex.Code is 27 or 291 || ex.Message.Contains("geoNear", StringComparison.OrdinalIgnoreCase))
        {
            LogGeoNearFallback(_logger, ex);
            var active = await ListCompatibleAsync<ParadaDoc>(
                "paradas",
                Builders<BsonDocument>.Filter.Eq("activo", true),
                ct).ConfigureAwait(false);
            hits = active
                .Where(p => GeoMath.DistanceKm(lat, lon, p.Ubicacion.Latitud, p.Ubicacion.Longitud) <= radioKm)
                .ToList();
        }

        return hits
            .Select(p => (p, GeoMath.DistanceKm(lat, lon, p.Ubicacion.Latitud, p.Ubicacion.Longitud)))
            .OrderBy(x => x.Item2)
            .ToList();
    }

    public Task UpsertParadaAsync(ParadaDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Paradas.ReplaceOneAsync(p => p.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<IReadOnlyList<HorarioDoc>> ListHorariosAsync(Guid? rutaId, CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        var filter = rutaId.HasValue ? Builders<HorarioDoc>.Filter.Eq(h => h.RutaId, rutaId.Value) : Builders<HorarioDoc>.Filter.Empty;
        return await Horarios.Find(filter).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task UpsertHorarioAsync(HorarioDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Horarios.ReplaceOneAsync(h => h.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<HorarioDoc?> GetHorarioByIdAsync(string id, CancellationToken ct) =>
        await Horarios.Find(h => h.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<TarifaDoc>> ListTarifasAsync(Guid? rutaId, CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        var filter = rutaId.HasValue ? Builders<TarifaDoc>.Filter.Eq(t => t.RutaId, rutaId.Value) : Builders<TarifaDoc>.Filter.Empty;
        return await Tarifas.Find(filter).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task UpsertTarifaAsync(TarifaDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Tarifas.ReplaceOneAsync(t => t.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<TarifaDoc?> GetTarifaByIdAsync(string id, CancellationToken ct) =>
        await Tarifas.Find(t => t.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DestinoDoc>> ListDestinosAsync(CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await Destinos.Find(Builders<DestinoDoc>.Filter.Empty).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<DestinoDoc?> GetDestinoByIdAsync(string id, CancellationToken ct) =>
        await Destinos.Find(d => d.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task UpsertDestinoAsync(DestinoDoc doc, CancellationToken ct)
    {
        CatalogUpdatedAtUtc = DateTime.UtcNow;
        return Destinos.ReplaceOneAsync(d => d.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);
    }

    public async Task<IReadOnlyList<CalificacionDoc>> ListCalificacionesAsync(Guid? rutaId, Guid? empresaId, string? destinoId, Guid? usuarioId, CancellationToken ct)
    {
        var fb = Builders<CalificacionDoc>.Filter;
        var list = new List<FilterDefinition<CalificacionDoc>>();
        if (rutaId.HasValue) list.Add(fb.Eq(c => c.RutaId, rutaId.Value));
        if (empresaId.HasValue) list.Add(fb.Eq(c => c.EmpresaId, empresaId.Value));
        if (!string.IsNullOrWhiteSpace(destinoId)) list.Add(fb.Eq(c => c.DestinoId, destinoId.Trim()));
        if (usuarioId.HasValue) list.Add(fb.Eq(c => c.UsuarioId, usuarioId.Value));
        var combined = list.Count == 0 ? fb.Empty : fb.And(list);
        return await Calificaciones.Find(combined).SortByDescending(c => c.FechaCreacion).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<CalificacionDoc?> GetCalificacionByIdAsync(string id, CancellationToken ct) =>
        await Calificaciones.Find(c => c.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task UpsertCalificacionAsync(CalificacionDoc doc, CancellationToken ct) =>
        Calificaciones.ReplaceOneAsync(c => c.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public Task DeleteCalificacionAsync(string id, CancellationToken ct) =>
        Calificaciones.DeleteOneAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ViajeGuardadoDoc>> ListViajesUsuarioAsync(Guid usuarioId, CancellationToken ct) =>
        await Viajes.Find(v => v.UsuarioId == usuarioId).SortByDescending(v => v.FechaActualizacion).ToListAsync(ct).ConfigureAwait(false);

    public async Task<ViajeGuardadoDoc?> GetViajeByIdAsync(string id, CancellationToken ct) =>
        await Viajes.Find(v => v.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task UpsertViajeAsync(ViajeGuardadoDoc doc, CancellationToken ct) =>
        Viajes.ReplaceOneAsync(v => v.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public Task DeleteViajeAsync(string id, CancellationToken ct) =>
        Viajes.DeleteOneAsync(v => v.Id == id, ct);

    public async Task<ConsentimientoSensorDoc?> GetConsentimientoAsync(Guid usuarioId, CancellationToken ct) =>
        await Consentimientos.Find(c => c.UsuarioId == usuarioId).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task UpsertConsentimientoAsync(ConsentimientoSensorDoc doc, CancellationToken ct) =>
        Consentimientos.ReplaceOneAsync(c => c.UsuarioId == doc.UsuarioId, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public Task CreateLoteSensorAsync(LoteSensorDoc doc, CancellationToken ct) =>
        Lotes.InsertOneAsync(doc, cancellationToken: ct);

    public async Task<IReadOnlyList<LoteSensorDoc>> ListLotesSensorAsync(Guid? rutaId, CancellationToken ct)
    {
        var filter = rutaId.HasValue ? Builders<LoteSensorDoc>.Filter.Eq(x => x.RutaId, rutaId.Value) : Builders<LoteSensorDoc>.Filter.Empty;
        return await Lotes.Find(filter).SortByDescending(x => x.CreadoEnUtc).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task CreateCandidatoParadaAsync(CandidatoParadaDoc doc, CancellationToken ct) =>
        Candidatos.InsertOneAsync(doc, cancellationToken: ct);

    public async Task<CandidatoParadaDoc?> GetCandidatoParadaByIdAsync(string id, CancellationToken ct) =>
        await Candidatos.Find(c => c.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<CandidatoParadaDoc>> ListCandidatosParadaAsync(string? estado, Guid? rutaId, CancellationToken ct)
    {
        var fb = Builders<CandidatoParadaDoc>.Filter;
        var list = new List<FilterDefinition<CandidatoParadaDoc>>();
        if (!string.IsNullOrWhiteSpace(estado)) list.Add(fb.Eq(c => c.Estado, estado.Trim()));
        if (rutaId.HasValue) list.Add(fb.Eq(c => c.RutaId, rutaId.Value));
        var combined = list.Count == 0 ? fb.Empty : fb.And(list);
        return await Candidatos.Find(combined).SortByDescending(c => c.CreadoEnUtc).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task UpdateCandidatoParadaAsync(CandidatoParadaDoc doc, CancellationToken ct) =>
        Candidatos.ReplaceOneAsync(c => c.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public Task CreateRecomendacionAsync(SolicitudRecomendacionDoc doc, CancellationToken ct) =>
        Recomendaciones.InsertOneAsync(doc, cancellationToken: ct);

    public async Task<SolicitudRecomendacionDoc?> GetRecomendacionByIdAsync(string id, CancellationToken ct) =>
        await Recomendaciones.Find(r => r.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<SolicitudRecomendacionDoc>> ListRecomendacionesAsync(Guid? usuarioId, CancellationToken ct)
    {
        var filter = usuarioId.HasValue ? Builders<SolicitudRecomendacionDoc>.Filter.Eq(r => r.UsuarioId, usuarioId.Value) : Builders<SolicitudRecomendacionDoc>.Filter.Empty;
        return await Recomendaciones.Find(filter).SortByDescending(r => r.CreadoEnUtc).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task UpdateRecomendacionAsync(SolicitudRecomendacionDoc doc, CancellationToken ct) =>
        Recomendaciones.ReplaceOneAsync(r => r.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public async Task<IReadOnlyList<FuenteDatoDoc>> ListFuentesAsync(CancellationToken ct)
    {
        await EnsureRoleSeededAsync(ct).ConfigureAwait(false);
        return await Fuentes.Find(Builders<FuenteDatoDoc>.Filter.Empty).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<FuenteDatoDoc?> GetFuenteByIdAsync(Guid id, CancellationToken ct) =>
        await Fuentes.Find(f => f.Id == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public Task UpsertFuenteAsync(FuenteDatoDoc doc, CancellationToken ct) =>
        Fuentes.ReplaceOneAsync(f => f.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public Task CreateImportacionAsync(ImportacionDoc doc, CancellationToken ct) =>
        Importaciones.InsertOneAsync(doc, cancellationToken: ct);

    public async Task<IReadOnlyList<ImportacionDoc>> ListImportacionesAsync(Guid? fuenteId, CancellationToken ct)
    {
        var filter = fuenteId.HasValue ? Builders<ImportacionDoc>.Filter.Eq(x => x.FuenteDatoId, fuenteId.Value) : Builders<ImportacionDoc>.Filter.Empty;
        return await Importaciones.Find(filter).SortByDescending(x => x.EjecutadoEnUtc).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task UpsertDispositivoFcmAsync(DispositivoFcmDoc doc, CancellationToken ct) =>
        Dispositivos.ReplaceOneAsync(d => d.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public async Task<IReadOnlyList<DispositivoFcmDoc>> ListDispositivosUsuarioAsync(Guid usuarioId, CancellationToken ct) =>
        await Dispositivos.Find(d => d.UsuarioId == usuarioId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<bool> DeleteDispositivoFcmAsync(string id, Guid usuarioId, CancellationToken ct)
    {
        var res = await Dispositivos.DeleteOneAsync(d => d.Id == id && d.UsuarioId == usuarioId, ct).ConfigureAwait(false);
        return res.DeletedCount > 0;
    }
}
