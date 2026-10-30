using ApiSDD.Data;
using ApiSDD.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnitTest.Common;

namespace DatabaseTest;

/// <summary>nv-04 Database: migration applies, uniqueness holds, rowversion moves.</summary>
[Collection("sql")]
public sealed class SDDUsuarioMappingTests : IAsyncLifetime
{
    private readonly MsSqlFixture _sql;

    public SDDUsuarioMappingTests(MsSqlFixture sql)
    {
        _sql = sql;
    }

    public async Task InitializeAsync()
    {
        using var db = Open();
        await db.Database.MigrateAsync().ConfigureAwait(true);
        await db.SDDUsuarios.ExecuteDeleteAsync().ConfigureAwait(true);
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
    public void MappingDeclaresKeysAndConcurrency()
    {
        using var db = Open();
        var entity = db.Model.FindEntityType(typeof(SDDUsuario));
        Assert.NotNull(entity);
        var key = entity.FindPrimaryKey();
        Assert.NotNull(key);
        key.Properties.Select(static p => p.Name).Should().Equal("id");
        entity.GetIndexes().Count(static index => index.IsUnique).Should().Be(2);
        var version = entity.FindProperty("RowVersion");
        Assert.NotNull(version);
        version.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateUserNameRejectedByDatabase()
    {
        using var db = Open();
        db.SDDUsuarios.Add(new SDDUsuario
        {
            strNombreUsuario = "Usuario01",
            strPWD = "x",
            strCorreoElectronico = "a@test.local",
        });
        db.SDDUsuarios.Add(new SDDUsuario
        {
            strNombreUsuario = "Usuario01",
            strPWD = "x",
            strCorreoElectronico = "b@test.local",
        });

        var act = () => db.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>().ConfigureAwait(true);
    }

    [Fact]
    public async Task RowVersionChangesOnUpdate()
    {
        byte[] first;
        using (var db = Open())
        {
            var entity = new SDDUsuario
            {
                strNombreUsuario = "Usuario02",
                strPWD = "x",
                strCorreoElectronico = "c@test.local",
            };
            db.SDDUsuarios.Add(entity);
            await db.SaveChangesAsync().ConfigureAwait(true);
            first = entity.RowVersion.ToArray();
        }

        byte[] second;
        using (var db = Open())
        {
            var entity = await db.SDDUsuarios.SingleAsync(static e => e.strNombreUsuario == "Usuario02")
                .ConfigureAwait(true);
            entity.strCorreoElectronico = "d@test.local";
            await db.SaveChangesAsync().ConfigureAwait(true);
            second = entity.RowVersion.ToArray();
        }

        second.Should().NotEqual(first);
    }
}
