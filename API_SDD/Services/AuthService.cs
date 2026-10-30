using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Data.Entities;
using ApiSDD.Dtos;
using ApiSDD.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ApiSDD.Services;

/// <summary>
/// nv-05: login with fake-hash anti-enumeration, DB lockout 5 to 15min and re-hash.
/// Every credential failure maps to the same 401; concurrency closes the gate.
/// </summary>
internal sealed class AuthService : IAuthService
{
    private const int MaxAttempts = 5;

    private const int LockMinutes = 15;

    // Zero-filled Argon2id PHC with default costs: valid shape, never matches.
    // Burns one KDF on unknown users so timing stays flat.
    private const string FakeHash =
        "$argon2id$v=19$m=65536,t=3,p=4$AAAAAAAAAAAAAAAAAAAAAA$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private readonly AppDbContext _db;

    private readonly IPasswordHasher _hasher;

    private readonly IJwtTokenService _jwt;

    private readonly TimeProvider _time;

    private readonly IValidator<LoginRequestDto> _validator;

    public AuthService(
        AppDbContext db,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        TimeProvider time,
        IValidator<LoginRequestDto> validator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

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
            catch (DbUpdateException ex)
            {
                _db.ChangeTracker.Clear();
                throw new UnauthorizedException("State conflict.", ex);
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
            });
        }
        else
        {
            gate.intIntentos += 1;
            if (gate.intIntentos >= MaxAttempts)
            {
                gate.dteBloqueoHasta = now.AddMinutes(LockMinutes);
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RefreshHashAsync(
        Data.Entities.SDDUsuario user,
        string plainPassword,
        CancellationToken cancellationToken)
    {
        if (!_hasher.NeedsRehash(user.strPWD))
        {
            return;
        }

        user.strPWD = _hasher.Hash(plainPassword);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
        }
    }

    private async Task ClearGateAsync(string userName, CancellationToken cancellationToken)
    {
        var gate = await _db.SDDSegBloqueo
            .FirstOrDefaultAsync(e => e.strNombreUsuario == userName, cancellationToken)
            .ConfigureAwait(false);
        if (gate is null)
        {
            return;
        }

        _db.SDDSegBloqueo.Remove(gate);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
        }
    }
}
