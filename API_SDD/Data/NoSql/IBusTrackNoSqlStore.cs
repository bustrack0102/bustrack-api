namespace ApiSDD.Data.NoSql;

/// <summary>
/// Repositorio único NoSQL para el dominio completo de BusTrack (MongoDB Atlas en producción y almacén en memoria en tests aislados).
/// </summary>
public interface IBusTrackNoSqlStore
{
    DateTime CatalogUpdatedAtUtc { get; }

    // --- Auth y Usuarios (EP-AU) ---
    Task EnsureRoleSeededAsync(CancellationToken ct);
    Task<UsuarioDoc?> GetUsuarioByEmailAsync(string emailNormalized, CancellationToken ct);
    Task<UsuarioDoc?> GetUsuarioByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsUsuarioByEmailAsync(string emailNormalized, Guid? excludeId, CancellationToken ct);
    Task<(IReadOnlyList<UsuarioDoc> Items, int Total)> ListUsuariosAdminAsync(
        string? q, string? rol, bool? activo, int page, int pageSize, string? sortBy, string? dir, CancellationToken ct);
    Task CreateUsuarioAsync(UsuarioDoc usuario, CancellationToken ct);
    Task UpdateUsuarioAsync(UsuarioDoc usuario, CancellationToken ct);
    Task<RefreshTokenDoc?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct);
    Task CreateRefreshTokenAsync(RefreshTokenDoc token, CancellationToken ct);
    Task UpdateRefreshTokenAsync(RefreshTokenDoc token, CancellationToken ct);
    Task RevokeAllRefreshTokensForUserAsync(Guid usuarioId, DateTime revokedAtUtc, CancellationToken ct);
    Task<TokenRecuperacionDoc?> GetRecoveryTokenByHashAsync(string tokenHash, CancellationToken ct);
    Task InvalidateActiveRecoveryTokensAsync(Guid usuarioId, CancellationToken ct);
    Task CreateRecoveryTokenAsync(TokenRecuperacionDoc token, CancellationToken ct);
    Task UpdateRecoveryTokenAsync(TokenRecuperacionDoc token, CancellationToken ct);
    Task<BloqueoLoginDoc?> GetLoginLockAsync(string key, CancellationToken ct);
    Task UpsertLoginLockAsync(BloqueoLoginDoc doc, CancellationToken ct);
    Task DeleteLoginLockAsync(string key, CancellationToken ct);

    // --- Empresas y Medios (EP-EM) ---
    Task<IReadOnlyList<EmpresaDoc>> ListEmpresasAsync(CancellationToken ct);
    Task<EmpresaDoc?> GetEmpresaByIdAsync(Guid id, CancellationToken ct);
    Task UpsertEmpresaAsync(EmpresaDoc doc, CancellationToken ct);
    Task<IReadOnlyList<MedioTransporteDoc>> ListMediosTransporteAsync(CancellationToken ct);
    Task<MedioTransporteDoc?> GetMedioByIdAsync(int id, CancellationToken ct);
    Task<MedioTransporteDoc> CreateMedioTransporteAsync(MedioTransporteDoc doc, CancellationToken ct);
    Task UpdateMedioTransporteAsync(MedioTransporteDoc doc, CancellationToken ct);

    // --- Rutas, Paradas, Mapa (EP-RP) ---
    Task<IReadOnlyList<RutaDoc>> ListRutasAsync(CancellationToken ct);
    Task<RutaDoc?> GetRutaByIdAsync(Guid id, CancellationToken ct);
    Task UpsertRutaAsync(RutaDoc doc, CancellationToken ct);
    Task<IReadOnlyList<ParadaDoc>> ListParadasAsync(CancellationToken ct);
    Task<ParadaDoc?> GetParadaByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<(ParadaDoc Parada, double DistanciaKm)>> FindParadasNearAsync(double lat, double lon, double radioKm, CancellationToken ct);
    Task UpsertParadaAsync(ParadaDoc doc, CancellationToken ct);

    // --- Horarios y Tarifas (EP-HT) ---
    Task<IReadOnlyList<HorarioDoc>> ListHorariosAsync(Guid? rutaId, CancellationToken ct);
    Task UpsertHorarioAsync(HorarioDoc doc, CancellationToken ct);
    Task<HorarioDoc?> GetHorarioByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<TarifaDoc>> ListTarifasAsync(Guid? rutaId, CancellationToken ct);
    Task UpsertTarifaAsync(TarifaDoc doc, CancellationToken ct);
    Task<TarifaDoc?> GetTarifaByIdAsync(string id, CancellationToken ct);

    // --- Destinos (EP-DE) ---
    Task<IReadOnlyList<DestinoDoc>> ListDestinosAsync(CancellationToken ct);
    Task<DestinoDoc?> GetDestinoByIdAsync(string id, CancellationToken ct);
    Task UpsertDestinoAsync(DestinoDoc doc, CancellationToken ct);

    // --- Calificaciones y Viajes (EP-CA, EP-VG) ---
    Task<IReadOnlyList<CalificacionDoc>> ListCalificacionesAsync(Guid? rutaId, Guid? empresaId, string? destinoId, Guid? usuarioId, CancellationToken ct);
    Task<CalificacionDoc?> GetCalificacionByIdAsync(string id, CancellationToken ct);
    Task UpsertCalificacionAsync(CalificacionDoc doc, CancellationToken ct);
    Task DeleteCalificacionAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<ViajeGuardadoDoc>> ListViajesUsuarioAsync(Guid usuarioId, CancellationToken ct);
    Task<ViajeGuardadoDoc?> GetViajeByIdAsync(string id, CancellationToken ct);
    Task UpsertViajeAsync(ViajeGuardadoDoc doc, CancellationToken ct);
    Task DeleteViajeAsync(string id, CancellationToken ct);

    // --- ML y GenAI (EP-ML, EP-IA) ---
    Task<ConsentimientoSensorDoc?> GetConsentimientoAsync(Guid usuarioId, CancellationToken ct);
    Task UpsertConsentimientoAsync(ConsentimientoSensorDoc doc, CancellationToken ct);
    Task CreateLoteSensorAsync(LoteSensorDoc doc, CancellationToken ct);
    Task<IReadOnlyList<LoteSensorDoc>> ListLotesSensorAsync(Guid? rutaId, CancellationToken ct);
    Task CreateCandidatoParadaAsync(CandidatoParadaDoc doc, CancellationToken ct);
    Task<CandidatoParadaDoc?> GetCandidatoParadaByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<CandidatoParadaDoc>> ListCandidatosParadaAsync(string? estado, Guid? rutaId, CancellationToken ct);
    Task UpdateCandidatoParadaAsync(CandidatoParadaDoc doc, CancellationToken ct);
    Task CreateRecomendacionAsync(SolicitudRecomendacionDoc doc, CancellationToken ct);
    Task<SolicitudRecomendacionDoc?> GetRecomendacionByIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<SolicitudRecomendacionDoc>> ListRecomendacionesAsync(Guid? usuarioId, CancellationToken ct);
    Task UpdateRecomendacionAsync(SolicitudRecomendacionDoc doc, CancellationToken ct);

    // --- Admin y Push (EP-AD) ---
    Task<IReadOnlyList<FuenteDatoDoc>> ListFuentesAsync(CancellationToken ct);
    Task<FuenteDatoDoc?> GetFuenteByIdAsync(Guid id, CancellationToken ct);
    Task UpsertFuenteAsync(FuenteDatoDoc doc, CancellationToken ct);
    Task CreateImportacionAsync(ImportacionDoc doc, CancellationToken ct);
    Task<IReadOnlyList<ImportacionDoc>> ListImportacionesAsync(Guid? fuenteId, CancellationToken ct);
    Task UpsertDispositivoFcmAsync(DispositivoFcmDoc doc, CancellationToken ct);
    Task<IReadOnlyList<DispositivoFcmDoc>> ListDispositivosUsuarioAsync(Guid usuarioId, CancellationToken ct);
    Task<bool> DeleteDispositivoFcmAsync(string id, Guid usuarioId, CancellationToken ct);
}
