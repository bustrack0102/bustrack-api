using ApiSDD.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

namespace ApiSDD.Health;

/// <summary>Check MongoDB Atlas: Ping acotado a 5s para validar conectividad NoSQL.</summary>
internal sealed class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _mongo;
    private readonly IConfiguration _configuration;

    public MongoDbHealthCheck(MongoDbContext mongo, IConfiguration configuration)
    {
        _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var isMongoPrimary = string.Equals(
            _configuration["Persistence:Provider"],
            "MongoDB",
            StringComparison.OrdinalIgnoreCase);

        if (!_mongo.IsConfigured)
        {
            return isMongoPrimary
                ? HealthCheckResult.Unhealthy("mongodb unconfigured.")
                : HealthCheckResult.Healthy("mongodb optional when SQL primary is active.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            var pingCommand = new BsonDocument("ping", 1);
            await _mongo.Database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: timeout.Token)
                .ConfigureAwait(false);
            return HealthCheckResult.Healthy("mongodb reachable.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("mongodb check timed out.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("mongodb unreachable.", ex);
        }
    }
}
