using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ApiSDD.Data;
using ApiSDD.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Common;

namespace ContractTest;

/// <summary>nv-05 Contract: login wire keeps the legacy convention, no secret keys.</summary>
[Collection("sddsqlcontract")]
public sealed class AuthContractTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public AuthContractTests(ApiFactory factory, MsSqlFixture sql)
    {
        _factory = factory;
        _sql = sql;
    }

    public async Task InitializeAsync()
    {
        _sqlFactory = _factory.WithConnectionString(_sql.ConnectionString)
            .WithWebHostBuilder(static b => b.UseSetting("RateLimiting:LoginPer5Minutes", "100"));
        using var scope = _sqlFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync().ConfigureAwait(true);
        await db.SDDSegBloqueo.ExecuteDeleteAsync().ConfigureAwait(true);
        await db.SDDUsuarios.ExecuteDeleteAsync().ConfigureAwait(true);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task LoginShapeIsConventional()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var anonymous = _sqlFactory.CreateClient();
        var response = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto(TestDataFactory.UserName, TestDataFactory.Password)).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);

        var keys = doc.RootElement.EnumerateObject()
            .Select(static p => p.Name).OrderBy(static k => k, StringComparer.Ordinal).ToArray();
        keys.Should().Equal("dteExpira", "strToken");
        body.Should().NotContain("TokenString");
        body.Should().NotContain("strPWD");
        body.Should().NotContain(TestDataFactory.Password);
    }

    [Fact]
    public async Task LogoutReturnsEmpty204()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var anonymous = _sqlFactory.CreateClient();
        var login = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto(TestDataFactory.UserName, TestDataFactory.Password)).ConfigureAwait(true);
        var payload = await login.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(payload);
        var token = doc.RootElement.GetProperty("strToken").GetString()
            ?? throw new InvalidOperationException("Missing token.");

        var bearer = _sqlFactory.CreateClient();
        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var logout = await bearer.PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative), null).ConfigureAwait(true);

        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await logout.Content.ReadAsStringAsync().ConfigureAwait(true)).Should().BeEmpty();
    }
}
