using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiSDD.Data;
using ApiSDD.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>nv-06 Integration: login then logout revokes reuse on real SQL.</summary>
[Collection("sddsql")]
public sealed class AuthLogoutTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public AuthLogoutTests(ApiFactory factory, MsSqlFixture sql)
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

    private async Task<string> LoginAsAdminAsync()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Admin0001", email: "admin0001@test.local", role: "Admin"))
            .ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var anonymous = _sqlFactory.CreateClient();
        var login = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto("Admin0001", TestDataFactory.Password)).ConfigureAwait(true);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await login.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = System.Text.Json.JsonDocument.Parse(payload);
        return doc.RootElement.GetProperty("strToken").GetString()
            ?? throw new InvalidOperationException("Missing token.");
    }

    private static HttpClient BearerClient(WebApplicationFactory<Program> factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task LogoutChain204ThenRevoked401s()
    {
        var token = await LoginAsAdminAsync().ConfigureAwait(true);

        var before = await BearerClient(_sqlFactory, token).GetAsync(
            new Uri("/api/v1/sddusuarios?page=1&pageSize=5", UriKind.Relative)).ConfigureAwait(true);
        before.StatusCode.Should().Be(HttpStatusCode.OK);

        var logout = await BearerClient(_sqlFactory, token).PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative), null).ConfigureAwait(true);
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await logout.Content.ReadAsStringAsync().ConfigureAwait(true)).Should().BeEmpty();

        var reused = await BearerClient(_sqlFactory, token).GetAsync(
            new Uri("/api/v1/sddusuarios?page=1&pageSize=5", UriKind.Relative)).ConfigureAwait(true);
        reused.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var write = await BearerClient(_sqlFactory, token).PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Usuario02", email: "usuario02@test.local")).ConfigureAwait(true);
        write.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var repeated = await BearerClient(_sqlFactory, token).PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative), null).ConfigureAwait(true);
        repeated.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutWithoutToken401()
    {
        var anonymous = _sqlFactory.CreateClient();
        var response = await anonymous.PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative), null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UsuarioRoleLogout204()
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
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await login.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = System.Text.Json.JsonDocument.Parse(payload);
        var token = doc.RootElement.GetProperty("strToken").GetString()
            ?? throw new InvalidOperationException("Missing token.");

        var logout = await BearerClient(_sqlFactory, token).PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative), null).ConfigureAwait(true);
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
