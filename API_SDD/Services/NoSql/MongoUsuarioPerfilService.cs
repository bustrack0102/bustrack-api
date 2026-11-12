using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos;
using ApiSDD.Security;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Gestión de perfil propio y administración de usuarios sobre NoSQL (EP-AU-07 a EP-AU-10).
/// </summary>
internal sealed class MongoUsuarioPerfilService : IUsuarioPerfilService
{
    private readonly IBusTrackNoSqlStore _store;
    private readonly IPasswordHasher _hasher;

    public MongoUsuarioPerfilService(IBusTrackNoSqlStore store, IPasswordHasher hasher)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    }

    public async Task<UsuarioMeResponseDto> GetMeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await GetAuthenticatedUserAsync(principal, cancellationToken).ConfigureAwait(false);
        var viajes = await _store.ListViajesUsuarioAsync(user.Id, cancellationToken).ConfigureAwait(false);
        var calificaciones = await _store.ListCalificacionesAsync(null, null, user.Id, cancellationToken).ConfigureAwait(false);
        var consentimiento = await _store.GetConsentimientoAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return new UsuarioMeResponseDto(
            user.Id,
            user.NombreCompleto,
            user.Correo,
            user.RolNombre,
            user.FechaCreacion,
            consentimiento?.Activo ?? user.ConsentimientoSensores,
            viajes.Count,
            calificaciones.Count);
    }

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

            if (nuevoCorreo != user.Correo
                && await _store.ExistsUsuarioByEmailAsync(nuevoCorreo, user.Id, cancellationToken).ConfigureAwait(false))
            {
                throw new ConflictException("El correo electrónico ya está registrado por otra cuenta.");
            }

            user.Correo = nuevoCorreo;
        }

        if (!string.IsNullOrWhiteSpace(dto.nombreCompleto))
        {
            var nombre = dto.nombreCompleto.Trim();
            if (nombre.Length > 120)
            {
                throw new BadHttpRequestException("El nombre completo no puede exceder 120 caracteres.");
            }

            user.NombreCompleto = nombre;
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
        await _store.UpdateUsuarioAsync(user, cancellationToken).ConfigureAwait(false);

        return new UsuarioMeUpdateResponseDto(user.Id, user.NombreCompleto, user.Correo, user.FechaActualizacion);
    }

    public async Task<MessageResponseDto> DeleteMeAsync(
        ClaimsPrincipal principal,
        UsuarioMeDeleteRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var user = await GetAuthenticatedUserAsync(principal, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(dto.passwordConfirmacion)
            || !_hasher.Verify(user.PasswordHash, dto.passwordConfirmacion))
        {
            throw new BadHttpRequestException("Contraseña de confirmación incorrecta.");
        }

        var now = DateTime.UtcNow;
        user.Activo = false;
        user.NombreCompleto = "Usuario dado de baja";
        user.Correo = $"anon-{user.Id:N}@baja.bustrack.local";
        user.PasswordHash = _hasher.Hash(CryptoTokenHelper.GenerateSecureToken());
        user.ConsentimientoSensores = false;
        user.FechaActualizacion = now;

        await _store.UpdateUsuarioAsync(user, cancellationToken).ConfigureAwait(false);
        await _store.RevokeAllRefreshTokensForUserAsync(user.Id, now, cancellationToken).ConfigureAwait(false);
        await _store.InvalidateActiveRecoveryTokensAsync(user.Id, cancellationToken).ConfigureAwait(false);
        await _store.UpsertConsentimientoAsync(new ConsentimientoSensorDoc
        {
            UsuarioId = user.Id,
            Activo = false,
            VersionAviso = "baja",
            FechaActualizacion = now,
        }, cancellationToken).ConfigureAwait(false);

        return new MessageResponseDto("Tu cuenta y datos personales han sido dados de baja correctamente.");
    }

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
        var p = pagina < 1 ? 1 : pagina;
        var s = tamanoPagina is < 1 or > 100 ? 20 : tamanoPagina;
        var (items, total) = await _store.ListUsuariosAdminAsync(q, rol, activo, p, s, ordenarPor, direccion, cancellationToken)
            .ConfigureAwait(false);

        var mapped = items.Select(u => new UsuarioAdminItemDto(
            u.Id,
            u.NombreCompleto,
            u.Correo,
            u.RolNombre,
            u.Activo,
            u.UltimoAcceso)).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)s);
        return new PagedUsuariosAdminResponseDto(total, p, totalPages, mapped);
    }

    private async Task<UsuarioDoc> GetAuthenticatedUserAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        var sub = principal.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var usuarioId))
        {
            throw new UnauthorizedException();
        }

        var user = await _store.GetUsuarioByIdAsync(usuarioId, ct).ConfigureAwait(false);
        if (user is null || !user.Activo)
        {
            throw new UnauthorizedException();
        }

        return user;
    }
}
