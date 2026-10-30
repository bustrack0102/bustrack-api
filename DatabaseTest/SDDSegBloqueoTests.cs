using ApiSDD.Data;
using ApiSDD.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnitTest.Common;

namespace DatabaseTest;

/// <summary>nv-05 Database: lockout table applies, persists gates and closes on concurrency.</summary>
[Collection("sql")]
public sealed class SDDSegBloqueoTests : IAsyncLifetime
{
    private readonly MsSqlFixture _sql;

    public SDDSegBloqueoTests(MsSqlFixture sql)
    {
        _sql = sql;
    }

    public async Task InitializeAsync()
    {
        using var db = Open();
        await db.Database.MigrateAsync().ConfigureAwait(true);
        await db.SDDSegBloqueo.ExecuteDeleteAsync().ConfigureAwait(true);
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
    public void MappingDeclaresKeyAndConcurrency()
    {
        using var db = Open();
        var entity = db.Model.FindEntityType(typeof(SDDSegBloqueo));
        Assert.NotNull(entity);
        var key = entity.FindPrimaryKey();
        Assert.NotNull(key);
        key.Properties.Select(static p => p.Name).Should().Equal("strNombreUsuario");
        var version = entity.FindProperty("RowVersion");
        Assert.NotNull(version);
        version.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public async Task PersistsAttemptsAndLockout()
    {
        using (var db = Open())
        {
            db.SDDSegBloqueo.Add(new SDDSegBloqueo
            {
                strNombreUsuario = "Usuario01",
                intIntentos = 5,
                dteBloqueoHasta = new DateTime(2026, 10, 12, 12, 15, 0, DateTimeKind.Utc),
            });
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        using (var db = Open())
        {
            var gate = await db.SDDSegBloqueo
                .SingleAsync(static e => e.strNombreUsuario == "Usuario01").ConfigureAwait(true);
            gate.intIntentos.Should().Be(5);
            gate.dteBloqueoHasta.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ConcurrentWriteFailsClosed()
    {
        using (var db = Open())
        {
            db.SDDSegBloqueo.Add(new SDDSegBloqueo
            {
                strNombreUsuario = "Usuario02",
                intIntentos = 1,
            });
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        using var first = Open();
        using var second = Open();
        var a = await first.SDDSegBloqueo
            .SingleAsync(static e => e.strNombreUsuario == "Usuario02").ConfigureAwait(true);
        var b = await second.SDDSegBloqueo
            .SingleAsync(static e => e.strNombreUsuario == "Usuario02").ConfigureAwait(true);

        a.intIntentos = 2;
        await first.SaveChangesAsync().ConfigureAwait(true);

        b.intIntentos = 3;
        var act = () => second.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>().ConfigureAwait(true);
    }
}
