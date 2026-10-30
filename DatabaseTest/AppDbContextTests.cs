using System.Net;
using ApiSDD.Data;
using ApiSDD.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Common;

namespace DatabaseTest;

/// <summary>NV-01 Database: modelo vacío SqlServer sin contenedor + ready 200 con SQL real.</summary>
[Collection("sql")]
public sealed class AppDbContextTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;

    public AppDbContextTests(ApiFactory factory, MsSqlFixture sql)
    {
        _factory = factory;
        _sql = sql;
    }

    [Fact]
    public void ModeloRegistraSDDUsuario()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
        db.Model.FindEntityType(typeof(SDDUsuario)).Should().NotBeNull();
    }

    [Fact]
    public async Task Ready200ConSqlReal()
    {
        var client = _factory.WithConnectionString(_sql.ConnectionString).CreateClient();
        var response = await TestHttp.GetAsync(client, "/health/ready").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
