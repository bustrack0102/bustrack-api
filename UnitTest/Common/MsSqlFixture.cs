using Testcontainers.MsSql;

namespace UnitTest.Common;

/// <summary>Puerto fijo para SQL de tests (constitution §7). Override vía MSSQL_PORT_FIXED.</summary>
public static class TestPorts
{
    public static int Sql =>
        int.TryParse(Environment.GetEnvironmentVariable("MSSQL_PORT_FIXED"), out var port)
            ? port
            : 14333;
}

/// <summary>SQL Server real en contenedor (ver 02-test-data). Un contenedor por assembly.</summary>
public sealed class MsSqlFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPortBinding(TestPorts.Sql, 1433)
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}
