using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.MedioTransporte;
using ApiSDD.Services.MedioTransporte;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Implementación NoSQL de Medios de Transporte (EP-EM-09 a EP-EM-12) con caché distribuida.
/// </summary>
internal sealed class MongoMedioTransporteService : IMedioTransporteService
{
    private const string CachePrefix = "medios:nosql:";
    private const string CacheVersionKey = CachePrefix + "version";

    private readonly IBusTrackNoSqlStore _store;
    private readonly IDistributedCache _cache;

    public MongoMedioTransporteService(IBusTrackNoSqlStore store, IDistributedCache cache)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<IReadOnlyList<MedioTransporteItemDto>> ListAsync(
        bool? activo,
        string? municipio,
        bool? conRutasActivas,
        CancellationToken cancellationToken)
    {
        var version = await DistributedCacheHelper.GetVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        var key = $"{CachePrefix}v{version}:{activo}:{municipio}:{conRutasActivas}";

        return await DistributedCacheHelper.GetOrSetAsync(_cache, key, TimeSpan.FromSeconds(180), async () =>
        {
            var medios = await _store.ListMediosTransporteAsync(cancellationToken).ConfigureAwait(false);
            var rutas = (await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false)).Where(r => r.Activa).ToList();

            var query = medios.AsEnumerable();
            if (activo.HasValue)
            {
                query = query.Where(m => m.Activo == activo.Value);
            }

            if (conRutasActivas == true)
            {
                query = query.Where(m => rutas.Any(r => r.MedioTransporteId == m.Id));
            }

            if (!string.IsNullOrWhiteSpace(municipio))
            {
                var mun = municipio.Trim();
                query = query.Where(m => rutas.Any(r =>
                    r.MedioTransporteId == m.Id
                    && r.Municipio.Contains(mun, StringComparison.OrdinalIgnoreCase)));
            }

            return (IReadOnlyList<MedioTransporteItemDto>)query.OrderBy(m => m.Nombre).Select(m =>
                new MedioTransporteItemDto(
                    m.Id,
                    m.Clave,
                    m.Nombre,
                    m.Descripcion,
                    m.IconoClave,
                    rutas.Count(r => r.MedioTransporteId == m.Id),
                    m.FechaActualizacion)).ToList();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MedioTransporteItemDto> CreateAsync(
        MedioTransporteCreateDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.clave) || string.IsNullOrWhiteSpace(dto.nombre))
        {
            throw new BadHttpRequestException("La clave y el nombre del medio de transporte son obligatorios.");
        }

        var clave = dto.clave.Trim().ToUpperInvariant();
        var existing = await _store.ListMediosTransporteAsync(cancellationToken).ConfigureAwait(false);
        if (existing.Any(m => string.Equals(m.Clave, clave, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"Ya existe un medio de transporte con clave '{clave}'.");
        }

        var created = await _store.CreateMedioTransporteAsync(new MedioTransporteDoc
        {
            Clave = clave,
            Nombre = dto.nombre.Trim(),
            Descripcion = dto.descripcion?.Trim(),
            IconoClave = dto.iconoClave?.Trim(),
            Activo = true,
            FechaActualizacion = DateTime.UtcNow,
        }, cancellationToken).ConfigureAwait(false);

        await DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
        return new MedioTransporteItemDto(
            created.Id,
            created.Clave,
            created.Nombre,
            created.Descripcion,
            created.IconoClave,
            0,
            created.FechaActualizacion);
    }

    public async Task<MedioTransporteItemDto> UpdateAsync(
        int id,
        MedioTransporteUpdateDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var doc = await _store.GetMedioByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Medio de transporte {id} no encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.clave))
        {
            doc.Clave = dto.clave.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(dto.nombre))
        {
            doc.Nombre = dto.nombre.Trim();
        }

        doc.Descripcion = dto.descripcion?.Trim() ?? doc.Descripcion;
        doc.IconoClave = dto.iconoClave?.Trim() ?? doc.IconoClave;
        doc.Activo = dto.activo;
        doc.FechaActualizacion = DateTime.UtcNow;

        await _store.UpdateMedioTransporteAsync(doc, cancellationToken).ConfigureAwait(false);
        await DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);

        var rutasCount = (await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false))
            .Count(r => r.Activa && r.MedioTransporteId == id);
        return new MedioTransporteItemDto(
            doc.Id,
            doc.Clave,
            doc.Nombre,
            doc.Descripcion,
            doc.IconoClave,
            rutasCount,
            doc.FechaActualizacion);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var doc = await _store.GetMedioByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Medio de transporte {id} no encontrado.");

        var tieneRutas = (await _store.ListRutasAsync(cancellationToken).ConfigureAwait(false))
            .Any(r => r.Activa && r.MedioTransporteId == id);
        if (tieneRutas)
        {
            throw new ConflictException("No es posible desactivar un medio con rutas activas asociadas.");
        }

        doc.Activo = false;
        doc.FechaActualizacion = DateTime.UtcNow;
        await _store.UpdateMedioTransporteAsync(doc, cancellationToken).ConfigureAwait(false);
        await DistributedCacheHelper.InvalidateVersionAsync(_cache, CacheVersionKey, cancellationToken).ConfigureAwait(false);
    }
}
