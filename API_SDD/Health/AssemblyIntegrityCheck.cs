using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiSDD.Health;

/// <summary>
/// Integridad del ensamblado: SHA-256 del binario vs ExpectedSha256.
/// Ausente → Degraded Dev / Healthy Prod. Incorrecto → Unhealthy Prod / Degraded Dev.
/// </summary>
internal sealed class AssemblyIntegrityCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public AssemblyIntegrityCheck(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public static string ComputeSha256(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var expected = _configuration["Assembly:ExpectedSha256"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            return Task.FromResult(_environment.IsDevelopment()
                ? HealthCheckResult.Degraded("assembly integrity unconfigured.")
                : HealthCheckResult.Healthy("assembly integrity not enforced."));
        }

        var location = Assembly.GetEntryAssembly()?.Location;
        if (string.IsNullOrWhiteSpace(location) || !File.Exists(location))
        {
            return Task.FromResult(HealthCheckResult.Degraded("entry assembly not found."));
        }

        var actual = ComputeSha256(location);
        if (string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(HealthCheckResult.Healthy("assembly integrity verified."));
        }

        return Task.FromResult(_environment.IsProduction()
            ? HealthCheckResult.Unhealthy("assembly integrity mismatch.")
            : HealthCheckResult.Degraded("assembly integrity mismatch."));
    }
}
