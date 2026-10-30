using System.Globalization;
using System.Security.Claims;
using ApiSDD.Common;
using Microsoft.Extensions.Caching.Memory;

namespace ApiSDD.Security;

/// <summary>
/// nv-06: blacklist writer and reader. Expired tokens are a no-op; missing claims deny.
/// </summary>
internal sealed class TokenRevocationService : ITokenRevocationService
{
    private readonly IMemoryCache _cache;

    private readonly TimeProvider _time;

    public TokenRevocationService(IMemoryCache cache, TimeProvider time)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public Task RevokeAsync(string jti, DateTimeOffset exp, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(jti);
        var ttl = exp - _time.GetUtcNow();
        if (ttl <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        _cache.Set(KeyOf(jti), exp.ToUnixTimeSeconds(), new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
        });
        return Task.CompletedTask;
    }

    public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(jti);
        return Task.FromResult(_cache.TryGetValue(KeyOf(jti), out _));
    }

    public async Task RevokeCurrentAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var jti = user.FindFirst("jti")?.Value;
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw new UnauthorizedException("Missing jti.");
        }

        if (!long.TryParse(
                user.FindFirst("exp")?.Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var expUnix))
        {
            throw new UnauthorizedException("Missing exp.");
        }

        DateTimeOffset exp;
        try
        {
            exp = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new UnauthorizedException("State conflict.", ex);
        }

        await RevokeAsync(jti, exp, cancellationToken).ConfigureAwait(false);
    }

    private static string KeyOf(string jti) => ITokenRevocationService.KeyPrefix + jti;
}
