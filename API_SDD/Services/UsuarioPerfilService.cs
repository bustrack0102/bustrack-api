using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Dtos;
using ApiSDD.Security;
using Microsoft.EntityFrameworkCore;

namespace ApiSDD.Services;

/// <summary>
/// Implementación de gestión de perfil y administración de usuarios (EP-AU-07 a EP-AU-10).
/// </summary>
internal sealed class UsuarioPerfilService : IUsuarioPerfilService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UsuarioPerfilService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    }

    // EP-AU-07: Perfil propio
    public async Task<UsuarioMeResponseDto> GetMeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await GetAuthenticatedUserAsync(principal, cancellationToken).ConfigureAwait(false);
        var rolNombre = user.Rol?.Nombre ?? "Usuario";

        return new UsuarioMeResponseDto(
            user.Id,
            user.NombreCompleto,
            user.Correo,
            rolNombre,
            user.FechaCreacion,
            user.ConsentimientoSensores,
            0, // totalViajesGuardados (módulo viajes pendiente)
            0  // totalCalificaciones (módulo calificaciones pendiente)
        );
    }

    // EP-AU-08: Actualizar perfil propio
    public async Task<UsuarioMeUpdateResponseDto> UpdateMeAsync(
        ClaimsPrincipal principal,
        UsuarioMeUpdateRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var user = await GetAuthenticatedUserAsync(principal, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(dto.correo))
        {
            var nuevoCorreo = dto.correo.Trim().ToLowerInvariant();
            if (!PasswordPolicy.IsValidEmail(nuevoCorreo))
            {
                throw new BadHttpRequestException("El nuevo correo electrónico no tiene un formato válido.");
            }

            if (nuevoCorreo != user.Correo)
            {
                var existe = await _db.Usuarios.AnyAsync(u => u.Correo == nuevoCorreo && u.Id != user.Id, cancellationToken).ConfigureAwait(false);
                if (existe)
                {
                    throw new ConflictException("El correo electrónico ya está registrado por otra cuenta.");
                }
                user.Correo = nuevoCorreo;
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.nombreCompleto))
        {
            if (dto.nombreCompleto.Trim().Length > 120)
            {
                throw new BadHttpRequestException("El nombre completo no puede exceder 120 caracteres.");
            }
            user.NombreCompleto = dto.nombreCompleto.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.nuevaPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.passwordActual))
            {
                throw new BadHttpRequestException("Debe proporcionar la contraseña actual para establecer una nueva.");
            }

            if (!_hasher.Verify(user.PasswordHash, dto.passwordActual))
            {
                throw new BadHttpRequestException("La contraseña actual es incorrecta.");
            }

            PasswordPolicy.EnsureStrongPassword(dto.nuevaPassword);

            user.PasswordHash = _hasher.Hash(dto.nuevaPassword);
        }

        user.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UsuarioMeUpdateResponseDto(user.Id, user.NombreCompleto, user.Correo, user.FechaActualizacion);
    }

    // EP-AU-09: Baja de cuenta con anonimización de datos personales
    public async Task<MessageResponseDto> DeleteMeAsync(
        ClaimsPrincipal principal,
        UsuarioMeDeleteRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var user = await GetAuthenticatedUserAsync(principal, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(dto.passwordConfirmacion) || !_hasher.Verify(user.PasswordHash, dto.passwordConfirmacion))
        {
            throw new BadHttpRequestException("Contraseña de confirmación incorrecta.");
        }

        // Baja lógica + anonimización (EP-AU-09: conservar integridad estadística sin datos personales).
        var correoAnonimo = $"anon-{user.Id:N}@baja.bustrack.local";
        user.Activo = false;
        user.NombreCompleto = "Usuario dado de baja";
        user.Correo = correoAnonimo;
        user.PasswordHash = _hasher.Hash(CryptoTokenHelper.GenerateSecureToken());
        user.ConsentimientoSensores = false;
        user.FechaActualizacion = DateTime.UtcNow;

        // Revocar refresh tokens activos y eliminar tokens de recuperación pendientes
        var tokens = await _db.RefreshTokens.Where(rt => rt.UsuarioId == user.Id && !rt.Revocado).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var t in tokens)
        {
            t.Revocado = true;
            t.FechaRevocacion = DateTime.UtcNow;
        }

        var tokensRecuperacion = await _db.TokensRecuperacion
            .Where(tr => tr.UsuarioId == user.Id && !tr.Usado)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        _db.TokensRecuperacion.RemoveRange(tokensRecuperacion);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new MessageResponseDto("Tu cuenta y datos personales han sido dados de baja correctamente.");
    }

    // EP-AU-10: Listado administrativo multicriterio
    public async Task<PagedUsuariosAdminResponseDto> ListAdminAsync(
        string? q,
        string? rol,
        bool? activo,
        int pagina,
        int tamanoPagina,
        string? ordenarPor,
        string? direccion,
        CancellationToken cancellationToken)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 100 ? 20 : tamanoPagina;

        var query = _db.Usuarios.Include(u => u.Rol).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u => u.NombreCompleto.Contains(term) || u.Correo.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(rol))
        {
            var rolTerm = rol.Trim();
            query = query.Where(u => u.Rol != null && u.Rol.Nombre == rolTerm);
        }

        if (activo.HasValue)
        {
            query = query.Where(u => u.Activo == activo.Value);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var isDesc = string.Equals(direccion, "desc", StringComparison.OrdinalIgnoreCase);
        query = ordenarPor?.ToLowerInvariant() switch
        {
            "nombre" => isDesc ? query.OrderByDescending(u => u.NombreCompleto) : query.OrderBy(u => u.NombreCompleto),
            "correo" => isDesc ? query.OrderByDescending(u => u.Correo) : query.OrderBy(u => u.Correo),
            _ => isDesc ? query.OrderByDescending(u => u.FechaCreacion) : query.OrderBy(u => u.FechaCreacion)
        };

        var skip = (pagina - 1) * tamanoPagina;
        var list = await query.Skip(skip).Take(tamanoPagina).ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = list.Select(u => new UsuarioAdminItemDto(
            u.Id,
            u.NombreCompleto,
            u.Correo,
            u.Rol?.Nombre ?? "Usuario",
            u.Activo,
            u.UltimoAcceso
        )).ToList();

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina);
        return new PagedUsuariosAdminResponseDto(total, pagina, totalPaginas, items);
    }

    private async Task<Data.Entities.Usuario> GetAuthenticatedUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var sub = principal.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var usuarioId))
        {
            throw new UnauthorizedException();
        }

        var user = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !user.Activo)
        {
            throw new UnauthorizedException();
        }

        return user;
    }
}
