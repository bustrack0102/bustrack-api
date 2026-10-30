using System.IdentityModel.Tokens.Jwt;
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

namespace IntegrationTest;

/// <summary>nv-05 Integration: register then login, identical 401s and DB lockout on real SQL.</summary>
[Collection("sddsql")]
public sealed class AuthLoginTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public AuthLoginTests(ApiFactory factory, MsSqlFixture sql)
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

    private HttpClient AdminClient()
    {
        var client = _sqlFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        return client;
    }

    private static async Task<string> ErrorOfAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("error").GetString()
            ?? throw new InvalidOperationException("Missing error.");
    }

    private async Task RegisterAsync(string user = TestDataFactory.UserName)
    {
        var admin = AdminClient();
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: user)).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login200WithDecodableJwt()
    {
        await RegisterAsync().ConfigureAwait(true);
        var anonymous = _sqlFactory.CreateClient();

        var response = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto(TestDataFactory.UserName, TestDataFactory.Password)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        body.Should().NotContain(TestDataFactory.Password);
        body.Should().NotContain("strPWD");
        using var doc = JsonDocument.Parse(body);
        var token = doc.RootElement.GetProperty("strToken").GetString();
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.First(static c => c.Type == "sub").Value.Should().NotBeNullOrEmpty();
        jwt.Claims.First(static c => c.Type == "role").Value.Should().Be("Usuario");
        jwt.Claims.First(static c => c.Type == "jti").Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WrongPasswordAndUnknownUserShareIdentical401()
    {
        await RegisterAsync().ConfigureAwait(true);
        var anonymous = _sqlFactory.CreateClient();

        var wrong = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto(TestDataFactory.UserName, "Zz9#wxyz")).ConfigureAwait(true);
        var unknown = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto("Usuario99", "Zz9#wxyz")).ConfigureAwait(true);

        wrong.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        unknown.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ErrorOfAsync(wrong).ConfigureAwait(true)).Should().Be("invalid_credentials");
        (await ErrorOfAsync(unknown).ConfigureAwait(true)).Should().Be("invalid_credentials");
    }

    [Fact]
    public async Task FiveFailuresLockOutEvenCorrectPassword()
    {
        await RegisterAsync().ConfigureAwait(true);
        var anonymous = _sqlFactory.CreateClient();
        var path = new Uri("/api/v1/auth/login", UriKind.Relative);

        for (var i = 0; i < 5; i++)
        {
            var failed = await anonymous.PostAsJsonAsync(
                path, new LoginRequestDto(TestDataFactory.UserName, "Zz9#wxyz")).ConfigureAwait(true);
            failed.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var locked = await anonymous.PostAsJsonAsync(
            path, new LoginRequestDto(TestDataFactory.UserName, TestDataFactory.Password)).ConfigureAwait(true);
        locked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ErrorOfAsync(locked).ConfigureAwait(true)).Should().Be("invalid_credentials");
    }

    [Fact]
    public async Task BadShape422()
    {
        var anonymous = _sqlFactory.CreateClient();
        var response = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto("short", TestDataFactory.Password)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AdminRouteWithoutToken401Regression()
    {
        var anonymous = _sqlFactory.CreateClient();
        var response = await anonymous.GetAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
