using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Data.Entities;
using ApiSDD.Dtos;
using ApiSDD.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

// Service name follows the SDDUsuario entity it owns; Sonar S101 cannot apply.
#pragma warning disable S101

namespace ApiSDD.Services;

/// <summary>
/// nv-04: user slice. Normalizes at the boundary, keeps the stored date
/// immutable, and maps duplicates plus stale versions to 409.
/// </summary>
internal sealed class SDDUsuarioService : ISDDUsuarioService
{
    private readonly AppDbContext _db;

    private readonly IPasswordHasher _hasher;

    private readonly TimeProvider _time;

    private readonly IValidator<SDDUsuarioCreateDto> _createValidator;

    private readonly IValidator<SDDUsuarioUpdateDto> _updateValidator;

    private readonly IValidator<PaginationQuery> _pagingValidator;

    public SDDUsuarioService(
        AppDbContext db,
        IPasswordHasher hasher,
        TimeProvider time,
        IValidator<SDDUsuarioCreateDto> createValidator,
        IValidator<SDDUsuarioUpdateDto> updateValidator,
        IValidator<PaginationQuery> pagingValidator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _pagingValidator = pagingValidator ?? throw new ArgumentNullException(nameof(pagingValidator));
    }

    public async Task<SDDUsuarioReadDto> CreateAsync(
        SDDUsuarioCreateDto dto,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken).ConfigureAwait(false);

        var userName = dto.strNombreUsuario.Trim();
        var email = dto.strCorreoElectronico.Trim().ToLowerInvariant();
        var role = dto.strRol ?? "Usuario";
        if (role != "Usuario" && !isAdmin)
        {
            throw new ForbiddenException("Only admins assign roles.");
        }

        if (await ExistsUserNameAsync(userName, null, cancellationToken).ConfigureAwait(false)
            || await ExistsEmailAsync(email, null, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("User name or email already exists.");
        }

        var entity = new SDDUsuario
        {
            strNombreUsuario = userName,
            strPWD = _hasher.Hash(dto.strPWD),
            strCorreoElectronico = email,
            strRol = role,
            dteFechaRegistro = _time.GetUtcNow().UtcDateTime,
        };

        _db.SDDUsuarios.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(entity);
    }

    public async Task<SDDUsuarioReadDto> UpdateAsync(
        int id,
        SDDUsuarioUpdateDto dto,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken).ConfigureAwait(false);

        if (!RowVersionCodec.TryDecode(dto.RowVersion, out var version))
        {
            throw new RequestValidationException("RowVersion must be valid base64.");
        }

        var entity = await _db.SDDUsuarios
            .FirstOrDefaultAsync(e => e.id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (dto.strNombreUsuario is not null)
        {
            var userName = dto.strNombreUsuario.Trim();
            if (!string.Equals(entity.strNombreUsuario, userName, StringComparison.Ordinal)
                && await ExistsUserNameAsync(userName, id, cancellationToken).ConfigureAwait(false))
            {
                throw new ConflictException("User name already exists.");
            }

            entity.strNombreUsuario = userName;
        }

        if (dto.strCorreoElectronico is not null)
        {
            var email = dto.strCorreoElectronico.Trim().ToLowerInvariant();
            if (!string.Equals(entity.strCorreoElectronico, email, StringComparison.Ordinal)
                && await ExistsEmailAsync(email, id, cancellationToken).ConfigureAwait(false))
            {
                throw new ConflictException("Email already exists.");
            }

            entity.strCorreoElectronico = email;
        }

        if (dto.strPWD is not null)
        {
            entity.strPWD = _hasher.Hash(dto.strPWD);
        }

        if (dto.strRol is not null)
        {
            if (!isAdmin)
            {
                throw new ForbiddenException("Only admins assign roles.");
            }

            entity.strRol = dto.strRol;
        }

        _db.Entry(entity).Property(e => e.RowVersion).OriginalValue = version;
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("Stale RowVersion.", ex);
        }

        return Map(entity);
    }

    public async Task<PagedResult<SDDUsuarioReadDto>> ListAsync(
        PaginationQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        await _pagingValidator.ValidateAndThrowAsync(query, cancellationToken).ConfigureAwait(false);

        var total = await _db.SDDUsuarios.CountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await _db.SDDUsuarios
            .AsNoTracking()
            .OrderBy(static e => e.id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = rows.Select(Map).ToList();
        return new PagedResult<SDDUsuarioReadDto>(items, total, query.Page, query.PageSize);
    }

    public async Task<SDDUsuarioReadDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.SDDUsuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new NotFoundException("User not found.");
        }

        return Map(entity);
    }

    public async Task DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);
        var entity = await _db.SDDUsuarios
            .FirstOrDefaultAsync(e => e.id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new NotFoundException("User not found.");
        }

        _db.Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;
        _db.SDDUsuarios.Remove(entity);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("Stale RowVersion.", ex);
        }
    }

    private static SDDUsuarioReadDto Map(SDDUsuario entity) => new(
        entity.id,
        entity.strNombreUsuario,
        entity.strCorreoElectronico,
        entity.strRol,
        entity.dteFechaRegistro,
        RowVersionCodec.Encode(entity.RowVersion));

    private Task<bool> ExistsUserNameAsync(string userName, int? excludeId, CancellationToken cancellationToken)
    {
        var lowered = userName.ToLowerInvariant();
        return _db.SDDUsuarios.AnyAsync(
            e => e.strNombreUsuario.ToLower() == lowered && (!excludeId.HasValue || e.id != excludeId.Value),
            cancellationToken);
    }

    private Task<bool> ExistsEmailAsync(string email, int? excludeId, CancellationToken cancellationToken)
    {
        return _db.SDDUsuarios.AnyAsync(
            e => e.strCorreoElectronico == email && (!excludeId.HasValue || e.id != excludeId.Value),
            cancellationToken);
    }
}
