using ApiSDD.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

namespace ApiSDD.Health;

/// <summary>Check MongoDB Atlas: Ping acotado a 5s para validar conectividad NoSQL.</summary>
internal sealed class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _mongo;

    public MongoDbHealthCheck(MongoDbContext mongo)
    {
        _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
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
            var pingCommand = new BsonDocument("ping", 1);
            await _mongo.Database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: timeout.Token).ConfigureAwait(false);
            return HealthCheckResult.Healthy("mongodb reachable.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("mongodb check timed out.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"mongodb unreachable: {ex.Message}", ex);
        }
    }
}
