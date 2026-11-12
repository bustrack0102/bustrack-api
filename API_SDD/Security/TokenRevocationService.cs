using System.Globalization;
using System.Security.Claims;
using ApiSDD.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace ApiSDD.Security;

/// <summary>
/// nv-06 + Req 10/15: writer/reader de blacklist JWT.
/// Usa IDistributedCache (Redis en producción multi-réplica, DistributedMemoryCache en local)
/// y mantiene constructor IMemoryCache por compatibilidad con pruebas unitarias legacy.
/// </summary>
internal sealed class TokenRevocationService : ITokenRevocationService
{
    private readonly IDistributedCache? _distributedCache;
    private readonly IMemoryCache? _memoryCache;
    private readonly TimeProvider _time;

    public TokenRevocationService(IMemoryCache cache, TimeProvider time)
    {
        _memoryCache = cache ?? throw new ArgumentNullException(nameof(cache));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    [ActivatorUtilitiesConstructor]
    public TokenRevocationService(IDistributedCache distributedCache, TimeProvider time)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public async Task RevokeAsync(string jti, DateTimeOffset exp, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(jti);
        var ttl = exp - _time.GetUtcNow();
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        if (_distributedCache is not null)
        {
            await _distributedCache.SetStringAsync(
                KeyOf(jti),
                exp.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken).ConfigureAwait(false);
            return;
        }

        _memoryCache!.Set(KeyOf(jti), exp.ToUnixTimeSeconds(), new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
        });
    }

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(jti);
        if (_distributedCache is not null)
        {
            var raw = await _distributedCache.GetStringAsync(KeyOf(jti), cancellationToken).ConfigureAwait(false);
            return raw is not null;
        }

        return _memoryCache!.TryGetValue(KeyOf(jti), out _);
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
