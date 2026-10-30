using ApiSDD.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnitTest.Common;

namespace DatabaseTest;

/// <summary>nv-06 Database: no model drift, no new migrations in this slice.</summary>
[Collection("sql")]
public sealed class NoModelDriftTests : IAsyncLifetime
{
    private readonly MsSqlFixture _sql;

    public NoModelDriftTests(MsSqlFixture sql)
    {
        _sql = sql;
    }

    public async Task InitializeAsync()
    {
        using var db = Open();
        await db.Database.MigrateAsync().ConfigureAwait(true);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private AppDbContext Open()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_sql.ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task NoPendingMigrations()
    {
        using var db = Open();
        await db.Database.MigrateAsync().ConfigureAwait(true);

        (await db.Database.GetPendingMigrationsAsync().ConfigureAwait(true)).Should().BeEmpty();
    }
}
