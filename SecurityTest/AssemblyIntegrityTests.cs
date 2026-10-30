using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ApiSDD.Health;

namespace SecurityTest;

/// <summary>NV-01 Security/AssemblyIntegrity 4: correcto, incorrecto Prod/Dev, ausente.</summary>
public sealed class AssemblyIntegrityTests
{
    private sealed class TestEnvironment : IHostEnvironment
    {
        public TestEnvironment(string name)
        {
            EnvironmentName = name;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "API_SDD";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
    }

    private static AssemblyIntegrityCheck Arrange(string? expected, string environment)
    {
        var values = new Dictionary<string, string?>();
        if (expected is not null)
        {
            values["Assembly:ExpectedSha256"] = expected;
        }

        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new AssemblyIntegrityCheck(config, new TestEnvironment(environment));
    }

    private static string ActualHash()
    {
        var location = Assembly.GetEntryAssembly()?.Location;
        Assert.NotNull(location);
        return AssemblyIntegrityCheck.ComputeSha256(location);
    }

    private static async Task<HealthStatus> CheckAsync(AssemblyIntegrityCheck check)
    {
        var result = await check.CheckHealthAsync(
            new HealthCheckContext(), CancellationToken.None).ConfigureAwait(true);
        return result.Status;
    }

    [Fact]
    public async Task HashCorrectoHealthy()
    {
        var check = Arrange(ActualHash(), "Development");

        (await CheckAsync(check).ConfigureAwait(true)).Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task HashIncorrectoUnhealthyEnProd()
    {
        var check = Arrange(new string('0', 64), "Production");

        (await CheckAsync(check).ConfigureAwait(true)).Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task HashIncorrectoDegradedEnDev()
    {
        var check = Arrange(new string('0', 64), "Development");

        (await CheckAsync(check).ConfigureAwait(true)).Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task AusenteDegradedEnDev()
    {
        var check = Arrange(null, "Development");

        (await CheckAsync(check).ConfigureAwait(true)).Should().Be(HealthStatus.Degraded);
    }
}
