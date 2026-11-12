using System.Globalization;
using System.Text.Json;
using ApiSDD.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiSDD.Services.NoSql;

/// <summary>
/// Helper único para caché distribuida Redis con invalidación atómica por versión lógica.
/// Evita duplicación de lógica GetOrCreate/Invalidate entre servicios de transporte.
/// </summary>
internal static class DistributedCacheHelper
{
    public static async Task<long> GetVersionAsync(
        IDistributedCache cache,
        string versionKey,
        CancellationToken ct)
    {
        var raw = await cache.GetStringAsync(versionKey, ct).ConfigureAwait(false);
        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
            ? version
            : 0L;
    }

    public static async Task InvalidateVersionAsync(
        IDistributedCache cache,
        string versionKey,
        CancellationToken ct)
    {
        var current = await GetVersionAsync(cache, versionKey, ct).ConfigureAwait(false);
        await cache.SetStringAsync(
            versionKey,
            (current + 1).ToString(CultureInfo.InvariantCulture),
            ct).ConfigureAwait(false);
    }

    public static async Task<T> GetOrSetAsync<T>(
        IDistributedCache cache,
        string key,
        TimeSpan ttl,
        Func<Task<T>> factory,
        CancellationToken ct)
    {
        var cached = await cache.GetStringAsync(key, ct).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cached))
        {
            var value = JsonSerializer.Deserialize<T>(cached, ApiJson.Options);
            if (value is not null)
            {
                return value;
            }
        }

        var fresh = await factory().ConfigureAwait(false);
        if (fresh is not null)
        {
            var serialized = JsonSerializer.Serialize(fresh, ApiJson.Options);
            await cache.SetStringAsync(
                key,
                serialized,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                ct).ConfigureAwait(false);
        }

        return fresh;
    }
}
