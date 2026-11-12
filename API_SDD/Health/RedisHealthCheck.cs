using System.Diagnostics.CodeAnalysis;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiSDD.Health;

/// <summary>
/// Check Redis / caché distribuida: realiza una sonda ligera de escritura/lectura cuando Redis está configurado.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "A health check must report Degraded/Unhealthy rather than throwing unhandled exceptions.")]
internal sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;

    public RedisHealthCheck(IDistributedCache cache, IConfiguration configuration)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var redisCs = ConfigurationSanitizer.CleanSecret(_configuration["Redis:ConnectionString"]);
        var usingRedis = ConfigurationSanitizer.HasUsableValue(redisCs);
        var usingMongoCache = _cache is MongoDistributedCache;
        if (!usingRedis && !usingMongoCache)
        {
            return HealthCheckResult.Healthy("distributed-memory cache active.");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(3));
        try
        {
            const string probeKey = "bustrack:health:ping";
            await _cache.SetStringAsync(
                probeKey,
                "1",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) },
                cts.Token).ConfigureAwait(false);
            var val = await _cache.GetStringAsync(probeKey, cts.Token).ConfigureAwait(false);
            if (val != "1")
            {
                return HealthCheckResult.Degraded("distributed cache probe mismatch.");
            }

            return HealthCheckResult.Healthy(usingRedis ? "redis reachable." : "distributed-mongodb cache active.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded($"distributed cache unreachable (fallback activo): {ex.Message}", ex);
        }
    }
}
