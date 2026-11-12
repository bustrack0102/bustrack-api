using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos;
using ApiSDD.Security;
using ApiSDD.Services.Email;
using FluentValidation;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Implementación NoSQL de autenticación y gestión de sesiones/recuperación (EP-AU-01 a EP-AU-06).
/// </summary>
internal sealed class MongoAuthService : IAuthService
{
    private const int MaxAttempts = 5;
    private const int LockMinutes = 15;
    private const int AccessTokenDurationSeconds = 900;

    private const string FakeHash =
        "$argon2id$v=19$m=65536,t=3,p=4$AAAAAAAAAAAAAAAAAAAAAA$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private readonly IBusTrackNoSqlStore _store;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly ITokenRevocationService _revocation;
    private readonly IEmailNotificationService _email;
    private readonly TimeProvider _time;
    private readonly IValidator<LoginRequestDto> _validator;

    public MongoAuthService(
        IBusTrackNoSqlStore store,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        ITokenRevocationService revocation,
        IEmailNotificationService email,
        TimeProvider time,
        IValidator<LoginRequestDto> validator)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _revocation = revocation ?? throw new ArgumentNullException(nameof(revocation));
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

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

        await _store.EnsureRoleSeededAsync(cancellationToken).ConfigureAwait(false);
        var correoNorm = dto.correo.Trim().ToLowerInvariant();
        if (await _store.ExistsUsuarioByEmailAsync(correoNorm, null, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("El correo electrónico ya se encuentra registrado.");
        }

        var now = _time.GetUtcNow().UtcDateTime;
        var user = new UsuarioDoc
        {
            Id = Guid.NewGuid(),
            NombreCompleto = dto.nombreCompleto.Trim(),
            Correo = correoNorm,
            PasswordHash = _hasher.Hash(dto.password),
            RolNombre = "Usuario",
            Activo = true,
            FechaAvisoPrivacidad = now,
            FechaCreacion = now,
        };
        await _store.CreateUsuarioAsync(user, cancellationToken).ConfigureAwait(false);

        var rawRefresh = CryptoTokenHelper.GenerateSecureToken();
        await _store.CreateRefreshTokenAsync(new RefreshTokenDoc
        {
            Id = Guid.NewGuid(),
            UsuarioId = user.Id,
            TokenHash = CryptoTokenHelper.HashToken(rawRefresh),
            FechaExpiracion = now.AddDays(30),
            Revocado = false,
        }, cancellationToken).ConfigureAwait(false);

        var issued = _jwt.CreateAccessToken(user.Id.ToString(), user.RolNombre);
        return new AuthResponseDto(
            new UsuarioPerfilResumenDto(user.Id, user.NombreCompleto, user.Correo, user.RolNombre),
            issued.Token,
            rawRefresh,
            AccessTokenDurationSeconds);
    }

    public async Task<AuthResponseDto> LoginV2Async(LoginInputDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var identificador = (dto.correo ?? dto.strNombreUsuario ?? string.Empty).Trim().ToLowerInvariant();
        var password = dto.password ?? dto.strPWD ?? string.Empty;
        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(password))
        {
            throw new BadHttpRequestException("Credenciales incompletas.");
        }

        var now = _time.GetUtcNow().UtcDateTime;
        var gate = await _store.GetLoginLockAsync(identificador, cancellationToken).ConfigureAwait(false);
        if (gate?.BloqueoHasta.HasValue == true && gate.BloqueoHasta.Value > now)
        {
            throw new UnauthorizedException();
        }

        var user = await _store.GetUsuarioByEmailAsync(identificador, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.Activo)
        {
            _hasher.Verify(FakeHash, password);
            await RecordFailureAsync(identificador, gate, now, cancellationToken).ConfigureAwait(false);
            throw new UnauthorizedException();
        }

        if (!_hasher.Verify(user.PasswordHash, password))
        {
            await RecordFailureAsync(identificador, gate, now, cancellationToken).ConfigureAwait(false);
            throw new UnauthorizedException();
        }

        if (gate is not null)
        {
            await _store.DeleteLoginLockAsync(identificador, cancellationToken).ConfigureAwait(false);
        }

        if (_hasher.NeedsRehash(user.PasswordHash))
        {
            user.PasswordHash = _hasher.Hash(password);
        }

        user.UltimoAcceso = now;
        await _store.UpdateUsuarioAsync(user, cancellationToken).ConfigureAwait(false);

        var rawRefresh = CryptoTokenHelper.GenerateSecureToken();
        await _store.CreateRefreshTokenAsync(new RefreshTokenDoc
        {
            Id = Guid.NewGuid(),
            UsuarioId = user.Id,
            TokenHash = CryptoTokenHelper.HashToken(rawRefresh),
            DispositivoInfo = dto.dispositivoInfo,
            FechaExpiracion = now.AddDays(30),
            Revocado = false,
        }, cancellationToken).ConfigureAwait(false);

        var issued = _jwt.CreateAccessToken(user.Id.ToString(), user.RolNombre);
        return new AuthResponseDto(
            new UsuarioPerfilResumenDto(user.Id, user.NombreCompleto, user.Correo, user.RolNombre),
            issued.Token,
            rawRefresh,
            AccessTokenDurationSeconds);
    }

    public async Task<TokenRefreshResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.refreshToken))
        {
            throw new BadHttpRequestException("El refresh token es obligatorio.");
        }

        var now = _time.GetUtcNow().UtcDateTime;
        var existing = await _store.GetRefreshTokenByHashAsync(
            CryptoTokenHelper.HashToken(dto.refreshToken),
            cancellationToken).ConfigureAwait(false);

        if (existing is null || existing.Revocado || existing.FechaExpiracion <= now)
        {
            throw new UnauthorizedException();
        }

        var user = await _store.GetUsuarioByIdAsync(existing.UsuarioId, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.Activo)
        {
            throw new UnauthorizedException();
        }

        var newRawRefresh = CryptoTokenHelper.GenerateSecureToken();
        var replacement = new RefreshTokenDoc
        {
            Id = Guid.NewGuid(),
            UsuarioId = user.Id,
            TokenHash = CryptoTokenHelper.HashToken(newRawRefresh),
            FechaExpiracion = now.AddDays(30),
            Revocado = false,
        };

        existing.Revocado = true;
        existing.FechaRevocacion = now;
        existing.ReemplazadoPorTokenId = replacement.Id;

        await _store.UpdateRefreshTokenAsync(existing, cancellationToken).ConfigureAwait(false);
        await _store.CreateRefreshTokenAsync(replacement, cancellationToken).ConfigureAwait(false);

        var issued = _jwt.CreateAccessToken(user.Id.ToString(), user.RolNombre);
        return new TokenRefreshResponseDto(issued.Token, newRawRefresh, AccessTokenDurationSeconds);
    }

    public async Task<MessageResponseDto> LogoutV2Async(
        ClaimsPrincipal userPrincipal,
        LogoutRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (userPrincipal is not null)
        {
            await _revocation.RevokeCurrentAsync(userPrincipal, cancellationToken).ConfigureAwait(false);
        }

        var now = _time.GetUtcNow().UtcDateTime;
        if (!string.IsNullOrWhiteSpace(dto?.refreshToken))
        {
            var token = await _store.GetRefreshTokenByHashAsync(
                CryptoTokenHelper.HashToken(dto.refreshToken),
                cancellationToken).ConfigureAwait(false);
            if (token is not null && !token.Revocado)
            {
                token.Revocado = true;
                token.FechaRevocacion = now;
                await _store.UpdateRefreshTokenAsync(token, cancellationToken).ConfigureAwait(false);
            }
        }

        var sub = userPrincipal?.FindFirst("sub")?.Value;
        if (dto?.cerrarTodosLosDispositivos == true && Guid.TryParse(sub, out var usuarioId))
        {
            await _store.RevokeAllRefreshTokensForUserAsync(usuarioId, now, cancellationToken).ConfigureAwait(false);
        }

        return new MessageResponseDto("Sesión cerrada correctamente");
    }

    public async Task<MessageResponseDto> RecuperarPasswordAsync(
        RecuperarPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.correo))
        {
            throw new BadHttpRequestException("El correo es requerido.");
        }

        var correoNorm = dto.correo.Trim().ToLowerInvariant();
        var user = await _store.GetUsuarioByEmailAsync(correoNorm, cancellationToken).ConfigureAwait(false);
        if (user is not null && user.Activo)
        {
            await _store.InvalidateActiveRecoveryTokensAsync(user.Id, cancellationToken).ConfigureAwait(false);
            var rawToken = CryptoTokenHelper.GenerateSecureToken();
            var now = _time.GetUtcNow().UtcDateTime;
            await _store.CreateRecoveryTokenAsync(new TokenRecuperacionDoc
            {
                Id = Guid.NewGuid(),
                UsuarioId = user.Id,
                TokenHash = CryptoTokenHelper.HashToken(rawToken),
                FechaExpiracion = now.AddMinutes(30),
                Usado = false,
                FechaCreacion = now,
            }, cancellationToken).ConfigureAwait(false);

            await _email.SendPasswordResetAsync(user.Correo, rawToken, cancellationToken).ConfigureAwait(false);
        }

        return new MessageResponseDto("Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.");
    }

    public async Task<MessageResponseDto> RestablecerPasswordAsync(
        RestablecerPasswordRequestDto dto,
        CancellationToken cancellationToken)
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

        var now = _time.GetUtcNow().UtcDateTime;
        var recup = await _store.GetRecoveryTokenByHashAsync(
            CryptoTokenHelper.HashToken(dto.token),
            cancellationToken).ConfigureAwait(false);

        if (recup is null || recup.Usado || recup.FechaExpiracion <= now)
        {
            throw new BadHttpRequestException("El enlace de restablecimiento es inválido o ha expirado.");
        }

        var user = await _store.GetUsuarioByIdAsync(recup.UsuarioId, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.Activo)
        {
            throw new BadHttpRequestException("Usuario inválido o inactivo.");
        }

        recup.Usado = true;
        recup.FechaUso = now;
        await _store.UpdateRecoveryTokenAsync(recup, cancellationToken).ConfigureAwait(false);

        user.PasswordHash = _hasher.Hash(dto.nuevaPassword);
        user.FechaActualizacion = now;
        await _store.UpdateUsuarioAsync(user, cancellationToken).ConfigureAwait(false);
        await _store.RevokeAllRefreshTokensForUserAsync(user.Id, now, cancellationToken).ConfigureAwait(false);

        return new MessageResponseDto("Contraseña actualizada correctamente. Inicia sesión con tu nueva clave.");
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await _validator.ValidateAndThrowAsync(dto, cancellationToken).ConfigureAwait(false);
        var res = await LoginV2Async(
            new LoginInputDto(dto.strNombreUsuario, dto.strPWD, null, null, "legacy"),
            cancellationToken).ConfigureAwait(false);
        var exp = _time.GetUtcNow().UtcDateTime.AddSeconds(res.expiraEnSegundos);
        return new LoginResponseDto(res.accessToken, exp);
    }

    private Task RecordFailureAsync(string key, BloqueoLoginDoc? current, DateTime now, CancellationToken ct)
    {
        var intentos = (current?.Intentos ?? 0) + 1;
        DateTime? lockUntil = intentos >= MaxAttempts ? now.AddMinutes(LockMinutes) : null;
        return _store.UpsertLoginLockAsync(new BloqueoLoginDoc
        {
            NombreUsuario = key,
            Intentos = intentos,
            BloqueoHasta = lockUntil,
        }, ct);
    }
}
