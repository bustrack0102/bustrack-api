using ApiSDD.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiSDD.Health;

/// <summary>Check SQL real: CanConnect acotado a 5s para fallar rápido (503) sin DB.</summary>
internal sealed class SqlServerHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public SqlServerHealthCheck(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            var reachable = await _db.Database.CanConnectAsync(timeout.Token).ConfigureAwait(false);
            return reachable
                ? HealthCheckResult.Healthy("sql reachable.")
                : HealthCheckResult.Unhealthy("sql unreachable.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("sql check timed out.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("sql unreachable.", ex);
        }
    }
}
