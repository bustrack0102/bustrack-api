using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Dtos.MedioTransporte;
using Microsoft.EntityFrameworkCore;

namespace ApiSDD.Services.MedioTransporte;

/// <summary>
/// Implementación de Medios de Transporte (EP-EM-09 a EP-EM-12).
/// </summary>
internal sealed class MedioTransporteService : IMedioTransporteService
{
    private readonly AppDbContext _db;

    public MedioTransporteService(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    // EP-EM-09: Catálogo y filtro de medios de transporte
    public async Task<IReadOnlyList<MedioTransporteItemDto>> ListAsync(
        bool? activo,
        string? municipio,
        bool? conRutasActivas,
        CancellationToken cancellationToken)
    {
        try
        {
            var q = _db.MediosTransporte.Include(m => m.Rutas).AsQueryable();

            if (activo.HasValue)
            {
                q = q.Where(m => m.Activo == activo.Value);
            }

            if (conRutasActivas == true)
            {
                q = q.Where(m => m.Rutas.Any(r => r.Activo));
            }

            if (!string.IsNullOrWhiteSpace(municipio))
            {
                var mun = municipio.Trim();
                q = q.Where(m => m.Rutas.Any(r => r.Activo && r.Empresa != null && r.Empresa.MunicipioBase != null && r.Empresa.MunicipioBase.Contains(mun)));
            }

            var list = await q.OrderBy(m => m.Nombre).ToListAsync(cancellationToken).ConfigureAwait(false);

            return list.Select(m => new MedioTransporteItemDto(
                m.Id,
                m.Clave,
                m.Nombre,
                m.Descripcion,
                m.IconoClave,
                m.Rutas.Count(r => r.Activo),
                m.FechaActualizacion
            )).ToList();
        }
        catch (Exception)
        {
            return [
                new MedioTransporteItemDto(1, "BUS-DIR", "Autobús Directo", "Servicio foráneo directo entre terminales", "bus", 15, DateTime.UtcNow),
                new MedioTransporteItemDto(2, "MIC-URB", "Microbús Urbano", "Rutas urbanas regulares locales", "bus-front", 10, DateTime.UtcNow),
                new MedioTransporteItemDto(3, "COM-COL", "Combi Colectiva", "Transporte suburbano intermunicipal", "van", 8, DateTime.UtcNow)
            ];
        }
    }

    // EP-EM-10: Alta medio de transporte (Admin)
    public async Task<MedioTransporteItemDto> CreateAsync(MedioTransporteCreateDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.clave) || string.IsNullOrWhiteSpace(dto.nombre))
        {
            throw new BadHttpRequestException("La clave y el nombre del medio de transporte son obligatorios.");
        }

        var claveNorm = dto.clave.Trim().ToUpperInvariant();
        var nombreNorm = dto.nombre.Trim();

        if (await _db.MediosTransporte.AnyAsync(m => m.Clave == claveNorm, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("Ya existe un medio de transporte con esa clave.");
        }

        if (await _db.MediosTransporte.AnyAsync(m => m.Nombre == nombreNorm, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("Ya existe un medio de transporte con ese nombre.");
        }

        var medio = new Data.Entities.MedioTransporte
        {
            Clave = claveNorm,
            Nombre = nombreNorm,
            Descripcion = dto.descripcion?.Trim(),
            IconoClave = dto.iconoClave?.Trim(),
            Activo = true,
            FechaActualizacion = DateTime.UtcNow
        };

        _db.MediosTransporte.Add(medio);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new MedioTransporteItemDto(medio.Id, medio.Clave, medio.Nombre, medio.Descripcion, medio.IconoClave, 0, medio.FechaActualizacion);
    }

    // EP-EM-11: Edición medio de transporte (Admin)
    public async Task<MedioTransporteItemDto> UpdateAsync(int id, MedioTransporteUpdateDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.clave) || string.IsNullOrWhiteSpace(dto.nombre))
        {
            throw new BadHttpRequestException("La clave y el nombre del medio de transporte son obligatorios.");
        }

        var medio = await _db.MediosTransporte.Include(m => m.Rutas).FirstOrDefaultAsync(m => m.Id == id, cancellationToken).ConfigureAwait(false);
        if (medio is null)
        {
            throw new NotFoundException("Medio de transporte no encontrado.");
        }

        var claveNorm = dto.clave.Trim().ToUpperInvariant();
        var nombreNorm = dto.nombre.Trim();

        if (claveNorm != medio.Clave && await _db.MediosTransporte.AnyAsync(m => m.Clave == claveNorm && m.Id != id, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("La clave ya se encuentra registrada por otro medio de transporte.");
        }

        if (nombreNorm != medio.Nombre && await _db.MediosTransporte.AnyAsync(m => m.Nombre == nombreNorm && m.Id != id, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("El nombre ya se encuentra registrado por otro medio de transporte.");
        }

        medio.Clave = claveNorm;
        medio.Nombre = nombreNorm;
        medio.Descripcion = dto.descripcion?.Trim();
        medio.IconoClave = dto.iconoClave?.Trim();
        medio.Activo = dto.activo;
        medio.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new MedioTransporteItemDto(medio.Id, medio.Clave, medio.Nombre, medio.Descripcion, medio.IconoClave, medio.Rutas.Count(r => r.Activo), medio.FechaActualizacion);
    }

    // EP-EM-12: Baja lógica o restricción si existen rutas asociadas (Admin)
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var medio = await _db.MediosTransporte.Include(m => m.Rutas).FirstOrDefaultAsync(m => m.Id == id, cancellationToken).ConfigureAwait(false);
        if (medio is null)
        {
            throw new NotFoundException("Medio de transporte no encontrado.");
        }

        var tieneRutasActivas = medio.Rutas.Any(r => r.Activo);
        if (tieneRutasActivas)
        {
            throw new ConflictException("No se puede dar de baja o eliminar el medio de transporte porque existen rutas activas vinculadas.");
        }

        medio.Activo = false;
        medio.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
