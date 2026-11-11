using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Data.Entities;
using ApiSDD.Dtos;
using ApiSDD.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ApiSDD.Services;

/// <summary>
/// Servicio de autenticación completo (NV-05 + EP-AU-01 a EP-AU-06).
/// </summary>
internal sealed class AuthService : IAuthService
{
    private const int MaxAttempts = 5;
    private const int LockMinutes = 15;
    private const int AccessTokenDurationSeconds = 900; // 15 minutos

    private const string FakeHash =
        "$argon2id$v=19$m=65536,t=3,p=4$AAAAAAAAAAAAAAAAAAAAAA$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly ITokenRevocationService _tokenRevocation;
    private readonly TimeProvider _time;
    private readonly IValidator<LoginRequestDto> _validator;

    public AuthService(
        AppDbContext db,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        ITokenRevocationService tokenRevocation,
        TimeProvider time,
        IValidator<LoginRequestDto> validator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _tokenRevocation = tokenRevocation ?? throw new ArgumentNullException(nameof(tokenRevocation));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    // -------------------------------------------------------------------------
    // EP-AU-01: Registro de usuario
    // -------------------------------------------------------------------------
    public async Task<AuthResponseDto> RegistroAsync(RegistroRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (!dto.aceptaAvisoPrivacidad)
        {
            throw new BadHttpRequestException("Debe aceptar el aviso de privacidad para registrarse.");
        }

        if (string.IsNullOrWhiteSpace(dto.nombreCompleto) || dto.nombreCompleto.Trim().Length > 120)
        {
            throw new BadHttpRequestException("El nombre completo es obligatorio (máx. 120 caracteres).");
        }

        if (string.IsNullOrWhiteSpace(dto.correo) || !PasswordPolicy.IsValidEmail(dto.correo.Trim()))
        {
            throw new BadHttpRequestException("El correo electrónico es obligatorio y debe tener un formato válido.");
        }

        if (string.IsNullOrWhiteSpace(dto.password) || dto.password != dto.confirmacionPassword)
        {
            throw new BadHttpRequestException("La contraseña y su confirmación no coinciden.");
        }

        PasswordPolicy.EnsureStrongPassword(dto.password);

        var correoNorm = dto.correo.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(u => u.Correo == correoNorm, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("El correo electrónico ya se encuentra registrado.");
        }

        var rolUsuario = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Usuario", cancellationToken).ConfigureAwait(false);
        if (rolUsuario is null)
        {
            rolUsuario = new Rol { Nombre = "Usuario", Descripcion = "Usuario final del sistema", FechaCreacion = DateTime.UtcNow };
            _db.Roles.Add(rolUsuario);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var nuevoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NombreCompleto = dto.nombreCompleto.Trim(),
            Correo = correoNorm,
            PasswordHash = _hasher.Hash(dto.password),
            RolId = rolUsuario.Id,
            Activo = true,
            FechaAvisoPrivacidad = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Usuarios.Add(nuevoUsuario);

        var rawRefreshToken = CryptoTokenHelper.GenerateSecureToken();
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = nuevoUsuario.Id,
            TokenHash = CryptoTokenHelper.HashToken(rawRefreshToken),
            FechaExpiracion = DateTime.UtcNow.AddDays(30),
            Revocado = false
        };

        _db.RefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tokenData = _jwt.CreateAccessToken(nuevoUsuario.Id.ToString(), rolUsuario.Nombre);

        return new AuthResponseDto(
            new UsuarioPerfilResumenDto(nuevoUsuario.Id, nuevoUsuario.NombreCompleto, nuevoUsuario.Correo, rolUsuario.Nombre),
            tokenData.Token,
            rawRefreshToken,
            AccessTokenDurationSeconds
        );
    }

    // -------------------------------------------------------------------------
    // EP-AU-02: Login con Refresh Token y soporte retrocompatible
    // -------------------------------------------------------------------------
    public async Task<AuthResponseDto> LoginV2Async(LoginInputDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var identificador = (dto.correo ?? dto.strNombreUsuario ?? string.Empty).Trim().ToLowerInvariant();
        var password = dto.password ?? dto.strPWD ?? string.Empty;

        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(password))
        {
            throw new BadHttpRequestException("Credenciales incompletas.");
        }

        var user = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == identificador && u.Activo, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            _hasher.Verify(FakeHash, password);
            throw new UnauthorizedException();
        }

        if (!_hasher.Verify(user.PasswordHash, password))
        {
            throw new UnauthorizedException();
        }

        user.UltimoAcceso = DateTime.UtcNow;

        var rawRefreshToken = CryptoTokenHelper.GenerateSecureToken();
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = user.Id,
            TokenHash = CryptoTokenHelper.HashToken(rawRefreshToken),
            DispositivoInfo = dto.dispositivoInfo,
            FechaExpiracion = DateTime.UtcNow.AddDays(30),
            Revocado = false
        };

        _db.RefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var rolNombre = user.Rol?.Nombre ?? "Usuario";
        var tokenData = _jwt.CreateAccessToken(user.Id.ToString(), rolNombre);

        return new AuthResponseDto(
            new UsuarioPerfilResumenDto(user.Id, user.NombreCompleto, user.Correo, rolNombre),
            tokenData.Token,
            rawRefreshToken,
            AccessTokenDurationSeconds
        );
    }

    // -------------------------------------------------------------------------
    // EP-AU-03: Refresh con rotación criptográfica
    // -------------------------------------------------------------------------
    public async Task<TokenRefreshResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.refreshToken))
        {
            throw new BadHttpRequestException("El refresh token es obligatorio.");
        }

        var tokenHash = CryptoTokenHelper.HashToken(dto.refreshToken);
        var existingToken = await _db.RefreshTokens
            .Include(rt => rt.Usuario)
            .ThenInclude(u => u!.Rol)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken)
            .ConfigureAwait(false);

        if (existingToken is null || existingToken.Revocado || existingToken.FechaExpiracion <= DateTime.UtcNow)
        {
            throw new UnauthorizedException();
        }

        var usuario = existingToken.Usuario;
        if (usuario is null || !usuario.Activo)
        {
            throw new UnauthorizedException();
        }

        // Rotación segura: invalidar el token anterior
        existingToken.Revocado = true;
        existingToken.FechaRevocacion = DateTime.UtcNow;

        var nuevoRawRefreshToken = CryptoTokenHelper.GenerateSecureToken();
        var nuevoRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            TokenHash = CryptoTokenHelper.HashToken(nuevoRawRefreshToken),
            FechaExpiracion = DateTime.UtcNow.AddDays(30),
            Revocado = false
        };

        existingToken.ReemplazadoPorTokenId = nuevoRefreshTokenEntity.Id;
        _db.RefreshTokens.Add(nuevoRefreshTokenEntity);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var rolNombre = usuario.Rol?.Nombre ?? "Usuario";
        var tokenData = _jwt.CreateAccessToken(usuario.Id.ToString(), rolNombre);

        return new TokenRefreshResponseDto(
            tokenData.Token,
            nuevoRawRefreshToken,
            AccessTokenDurationSeconds
        );
    }

    // -------------------------------------------------------------------------
    // EP-AU-04: Logout con revocación en base de datos
    // -------------------------------------------------------------------------
    public async Task<MessageResponseDto> LogoutV2Async(ClaimsPrincipal userPrincipal, LogoutRequestDto dto, CancellationToken cancellationToken)
    {
        if (userPrincipal is not null)
        {
            await _tokenRevocation.RevokeCurrentAsync(userPrincipal, cancellationToken).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(dto?.refreshToken))
        {
            var hash = CryptoTokenHelper.HashToken(dto.refreshToken);
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken).ConfigureAwait(false);
            if (token is not null && !token.Revocado)
            {
                token.Revocado = true;
                token.FechaRevocacion = DateTime.UtcNow;
            }
        }

        var sub = userPrincipal?.FindFirst("sub")?.Value;
        if (dto?.cerrarTodosLosDispositivos == true && Guid.TryParse(sub, out var usuarioId))
        {
            var activeTokens = await _db.RefreshTokens
                .Where(rt => rt.UsuarioId == usuarioId && !rt.Revocado)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var t in activeTokens)
            {
                t.Revocado = true;
                t.FechaRevocacion = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new MessageResponseDto("Sesión cerrada correctamente");
    }

    // -------------------------------------------------------------------------
    // EP-AU-05: Recuperación de contraseña
    // -------------------------------------------------------------------------
    public async Task<MessageResponseDto> RecuperarPasswordAsync(RecuperarPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.correo))
        {
            throw new BadHttpRequestException("El correo es requerido.");
        }

        var correoNorm = dto.correo.Trim().ToLowerInvariant();
        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoNorm && u.Activo, cancellationToken).ConfigureAwait(false);

        if (user is not null)
        {
            // Invalidar tokens previos vigentes
            var vigentes = await _db.TokensRecuperacion
                .Where(t => t.UsuarioId == user.Id && !t.Usado && t.FechaExpiracion > DateTime.UtcNow)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var t in vigentes)
            {
                t.Usado = true;
            }

            var rawToken = CryptoTokenHelper.GenerateSecureToken();
            var recoveryToken = new TokenRecuperacion
            {
                Id = Guid.NewGuid(),
                UsuarioId = user.Id,
                TokenHash = CryptoTokenHelper.HashToken(rawToken),
                FechaExpiracion = DateTime.UtcNow.AddMinutes(30),
                Usado = false,
                FechaCreacion = DateTime.UtcNow
            };

            _db.TokensRecuperacion.Add(recoveryToken);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        // Respuesta siempre idéntica para evitar enumeración de cuentas
        return new MessageResponseDto("Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.");
    }

    // -------------------------------------------------------------------------
    // EP-AU-06: Restablecer contraseña
    // -------------------------------------------------------------------------
    public async Task<MessageResponseDto> RestablecerPasswordAsync(RestablecerPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.token))
        {
            throw new BadHttpRequestException("El token es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.nuevaPassword) || dto.nuevaPassword != dto.confirmacionPassword)
        {
            throw new BadHttpRequestException("Las contraseñas no coinciden.");
        }

        PasswordPolicy.EnsureStrongPassword(dto.nuevaPassword);

        var tokenHash = CryptoTokenHelper.HashToken(dto.token);
        var recup = await _db.TokensRecuperacion
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken)
            .ConfigureAwait(false);

        if (recup is null || recup.Usado || recup.FechaExpiracion <= DateTime.UtcNow)
        {
            throw new BadHttpRequestException("El enlace de restablecimiento es inválido o ha expirado.");
        }

        var usuario = recup.Usuario;
        if (usuario is null || !usuario.Activo)
        {
            throw new BadHttpRequestException("Usuario inválido o inactivo.");
        }

        recup.Usado = true;
        recup.FechaUso = DateTime.UtcNow;

        usuario.PasswordHash = _hasher.Hash(dto.nuevaPassword);
        usuario.FechaActualizacion = DateTime.UtcNow;

        // Revocar todos los refresh tokens activos del usuario por seguridad
        var refreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UsuarioId == usuario.Id && !rt.Revocado)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var rt in refreshTokens)
        {
            rt.Revocado = true;
            rt.FechaRevocacion = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new MessageResponseDto("Contraseña actualizada correctamente. Inicia sesión con tu nueva clave.");
    }

    // -------------------------------------------------------------------------
    // Legacy NV-05 Login
    // -------------------------------------------------------------------------
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await _validator.ValidateAndThrowAsync(dto, cancellationToken).ConfigureAwait(false);

        var userName = dto.strNombreUsuario.Trim();
        var user = await _db.SDDUsuarios
            .FirstOrDefaultAsync(e => e.strNombreUsuario == userName, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            _hasher.Verify(FakeHash, dto.strPWD);
            throw new UnauthorizedException();
        }

        var now = _time.GetUtcNow().UtcDateTime;
        var gate = await _db.SDDSegBloqueo
            .FirstOrDefaultAsync(e => e.strNombreUsuario == user.strNombreUsuario, cancellationToken)
            .ConfigureAwait(false);

        var verified = _hasher.Verify(user.strPWD, dto.strPWD);
        if (IsLocked(gate, now))
        {
            throw new UnauthorizedException();
        }

        if (!verified)
        {
            await RecordFailureAsync(user.strNombreUsuario, now, cancellationToken).ConfigureAwait(false);
            throw new UnauthorizedException();
        }

        await RefreshHashAsync(user, dto.strPWD, cancellationToken).ConfigureAwait(false);
        await ClearGateAsync(user.strNombreUsuario, cancellationToken).ConfigureAwait(false);

        var issued = _jwt.CreateAccessToken(user.id, user.strRol);
        return new LoginResponseDto(issued.Token, issued.Expires);
    }

    private static bool IsLocked(SDDSegBloqueo? gate, DateTime now) =>
        gate?.dteBloqueoHasta.HasValue == true && gate.dteBloqueoHasta.Value > now;

    private async Task RecordFailureAsync(string userName, DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                await ApplyFailureAsync(userName, now, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                _db.ChangeTracker.Clear();
            }
        }
    }

    private async Task ApplyFailureAsync(string userName, DateTime now, CancellationToken cancellationToken)
    {
        var gate = await _db.SDDSegBloqueo
            .FirstOrDefaultAsync(e => e.strNombreUsuario == userName, cancellationToken)
            .ConfigureAwait(false);

        if (gate is null)
        {
            _db.SDDSegBloqueo.Add(new SDDSegBloqueo
            {
                strNombreUsuario = userName,
                intIntentos = 1,
                dteBloqueoHasta = null,
            });
        }
        else
        {
            gate.intIntentos++;
            if (gate.intIntentos >= MaxAttempts)
            {
                gate.dteBloqueoHasta = now.AddMinutes(LockMinutes);
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ClearGateAsync(string userName, CancellationToken cancellationToken)
    {
        var gate = await _db.SDDSegBloqueo
            .FirstOrDefaultAsync(e => e.strNombreUsuario == userName, cancellationToken)
            .ConfigureAwait(false);

        if (gate is not null)
        {
            _db.SDDSegBloqueo.Remove(gate);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RefreshHashAsync(SDDUsuario user, string plain, CancellationToken cancellationToken)
    {
        if (_hasher.NeedsRehash(user.strPWD))
        {
            user.strPWD = _hasher.Hash(plain);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
