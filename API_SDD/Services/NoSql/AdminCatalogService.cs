using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Operational;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Gestión de fuentes de datos oficiales y bitácora de importaciones administrativas (EP-AD-02 a EP-AD-07).
/// </summary>
public interface IAdminCatalogService
{
    Task<IReadOnlyList<FuenteDatoItemDto>> ListFuentesAsync(bool? soloActivas, CancellationToken ct);
    Task<FuenteDatoItemDto> CreateFuenteAsync(FuenteDatoUpsertRequest request, CancellationToken ct);
    Task<FuenteDatoItemDto> UpdateFuenteAsync(Guid id, FuenteDatoUpsertRequest request, CancellationToken ct);
    Task DeactivateFuenteAsync(Guid id, CancellationToken ct);

    Task<ImportacionItemDto> ExecuteImportacionAsync(ClaimsPrincipal admin, ImportacionExecuteRequest request, CancellationToken ct);
    Task<IReadOnlyList<ImportacionItemDto>> ListImportacionesAsync(Guid? fuenteDatoId, CancellationToken ct);
}

internal sealed class AdminCatalogService : IAdminCatalogService
{
    private readonly IBusTrackNoSqlStore _store;
    private readonly ITransportCatalogService _catalog;
    private readonly TimeProvider _time;

    public AdminCatalogService(
        IBusTrackNoSqlStore store,
        ITransportCatalogService catalog,
        TimeProvider time)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public async Task<IReadOnlyList<FuenteDatoItemDto>> ListFuentesAsync(bool? soloActivas, CancellationToken ct)
    {
        var list = await _store.ListFuentesAsync(ct).ConfigureAwait(false);
        if (soloActivas == true)
        {
            list = list.Where(f => f.Activa).ToList();
        }

        return list.Select(MapFuente).ToList();
    }

    public async Task<FuenteDatoItemDto> CreateFuenteAsync(FuenteDatoUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateFuente(request);

        var doc = new FuenteDatoDoc
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            TipoFuente = request.TipoFuente.Trim(),
            Responsable = request.Responsable.Trim(),
            ReferenciaUrl = request.ReferenciaUrl?.Trim(),
            Activa = request.Activa,
            ActualizadaEnUtc = _time.GetUtcNow().UtcDateTime,
        };

        await _store.UpsertFuenteAsync(doc, ct).ConfigureAwait(false);
        return MapFuente(doc);
    }

    public async Task<FuenteDatoItemDto> UpdateFuenteAsync(Guid id, FuenteDatoUpsertRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateFuente(request);

        var existing = await _store.GetFuenteByIdAsync(id, ct).ConfigureAwait(false)
                       ?? throw new NotFoundException("Fuente de datos no encontrada.");

        existing.Nombre = request.Nombre.Trim();
        existing.TipoFuente = request.TipoFuente.Trim();
        existing.Responsable = request.Responsable.Trim();
        existing.ReferenciaUrl = request.ReferenciaUrl?.Trim();
        existing.Activa = request.Activa;
        existing.ActualizadaEnUtc = _time.GetUtcNow().UtcDateTime;

        await _store.UpsertFuenteAsync(existing, ct).ConfigureAwait(false);
        return MapFuente(existing);
    }

    public async Task DeactivateFuenteAsync(Guid id, CancellationToken ct)
    {
        var existing = await _store.GetFuenteByIdAsync(id, ct).ConfigureAwait(false)
                       ?? throw new NotFoundException("Fuente de datos no encontrada.");

        existing.Activa = false;
        existing.ActualizadaEnUtc = _time.GetUtcNow().UtcDateTime;
        await _store.UpsertFuenteAsync(existing, ct).ConfigureAwait(false);
    }

    public async Task<ImportacionItemDto> ExecuteImportacionAsync(
        ClaimsPrincipal admin,
        ImportacionExecuteRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var adminId = ResolveAdminAuditId(admin);

        var fuente = await _store.GetFuenteByIdAsync(request.FuenteDatoId, ct).ConfigureAwait(false)
                     ?? throw new NotFoundException("Fuente de datos no encontrada.");
        if (!fuente.Activa)
        {
            throw new RequestValidationException("No se puede importar desde una fuente inactiva.");
        }

        if (string.IsNullOrWhiteSpace(request.TipoEntidad) || request.RegistrosProcesados < 1)
        {
            throw new RequestValidationException("Especifica tipoEntidad y registrosProcesados >= 1.");
        }

        var doc = new ImportacionDoc
        {
            Id = $"imp-{Guid.NewGuid():N}",
            FuenteDatoId = request.FuenteDatoId,
            UsuarioAdminId = adminId,
            TipoEntidad = request.TipoEntidad.Trim(),
            RegistrosProcesados = request.RegistrosProcesados,
            Estado = "Completada",
            EjecutadoEnUtc = _time.GetUtcNow().UtcDateTime,
        };

        await _store.CreateImportacionAsync(doc, ct).ConfigureAwait(false);
        await _catalog.InvalidateCatalogCacheAsync(ct).ConfigureAwait(false);
        return MapImportacion(doc);
    }

    public async Task<IReadOnlyList<ImportacionItemDto>> ListImportacionesAsync(Guid? fuenteDatoId, CancellationToken ct)
    {
        var list = await _store.ListImportacionesAsync(fuenteDatoId, ct).ConfigureAwait(false);
        return list.Select(MapImportacion).ToList();
    }

    private static Guid ResolveAdminAuditId(ClaimsPrincipal admin)
    {
        var guid = admin.TryGetUserId();
        if (guid.HasValue)
        {
            return guid.Value;
        }

        if (!admin.IsAdmin())
        {
            throw new UnauthorizedException("Token de administrador inválido.");
        }

        var sub = admin.FindFirst("sub")?.Value ?? "admin";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("admin:" + sub));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static void ValidateFuente(FuenteDatoUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre)
            || string.IsNullOrWhiteSpace(request.TipoFuente)
            || string.IsNullOrWhiteSpace(request.Responsable))
        {
            throw new RequestValidationException("Nombre, tipoFuente y responsable son obligatorios.");
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenciaUrl)
            && (!Uri.TryCreate(request.ReferenciaUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            throw new RequestValidationException("La referenciaUrl debe ser una URL HTTP/HTTPS válida.");
        }
    }

    private static FuenteDatoItemDto MapFuente(FuenteDatoDoc d) =>
        new(d.Id, d.Nombre, d.TipoFuente, d.Responsable, d.ReferenciaUrl, d.Activa, d.ActualizadaEnUtc);

    private static ImportacionItemDto MapImportacion(ImportacionDoc d) =>
        new(d.Id, d.FuenteDatoId, d.UsuarioAdminId, d.TipoEntidad, d.RegistrosProcesados, d.Estado, d.EjecutadoEnUtc);
}
