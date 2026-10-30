using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using ApiSDD.Controllers.V1;
using ApiSDD.Data;
using ApiSDD.Dtos;
using ApiSDD.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using UnitTest.Common;

namespace SecurityTest;

/// <summary>nv-05 Security: login rate, issued-token access, strict JWT and secret scrubbing.</summary>
[Collection("sddsqlsec")]
public sealed class AuthSecurityTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public AuthSecurityTests(ApiFactory factory, MsSqlFixture sql)
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
    public async Task LoginRateLimit429Uniform()
    {
        var limited = _factory.WithConnectionString(_sql.ConnectionString)
            .WithWebHostBuilder(static b => b.UseSetting("RateLimiting:LoginPer5Minutes", "2"));
        using var scope = limited.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync().ConfigureAwait(true);
        var client = limited.CreateClient();
        var path = new Uri("/api/v1/auth/login", UriKind.Relative);

        (await client.PostAsJsonAsync(path, new LoginRequestDto("Usuario01", "Zz9#wxyz")).ConfigureAwait(true))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync(path, new LoginRequestDto("Usuario01", "Zz9#wxyz")).ConfigureAwait(true))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var rejected = await client.PostAsJsonAsync(
            path, new LoginRequestDto("Usuario01", "Zz9#wxyz")).ConfigureAwait(true);
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Headers.Should().Contain(h => h.Key == "Retry-After");
        var body = await rejected.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("error").GetString().Should().Be("rate_limited");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(429);
    }

    [Fact]
    public async Task IssuedAdminTokenReachesAdminRoute()
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
        using var doc = JsonDocument.Parse(payload);
        var token = doc.RootElement.GetProperty("strToken").GetString()
            ?? throw new InvalidOperationException("Missing token.");

        var bearer = _sqlFactory.CreateClient();
        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var listed = await bearer.GetAsync(
            new Uri("/api/v1/sddusuarios?page=1&pageSize=5", UriKind.Relative)).ConfigureAwait(true);
        listed.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LogoutRateLimit429Uniform()
    {
        var limited = _factory.WithConnectionString(_sql.ConnectionString)
            .WithWebHostBuilder(static b =>
            {
                b.UseSetting("RateLimiting:LoginPer5Minutes", "100");
                b.UseSetting("RateLimiting:GlobalPerMinute", "3");
            });
        using var scope = limited.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync().ConfigureAwait(true);
        await db.SDDSegBloqueo.ExecuteDeleteAsync().ConfigureAwait(true);
        await db.SDDUsuarios.ExecuteDeleteAsync().ConfigureAwait(true);

        var admin = limited.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Admin0002", email: "admin0002@test.local", role: "Admin"))
            .ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var anonymous = limited.CreateClient();
        var login = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto("Admin0002", TestDataFactory.Password)).ConfigureAwait(true);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await login.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(payload);
        var token = doc.RootElement.GetProperty("strToken").GetString()
            ?? throw new InvalidOperationException("Missing token.");

        var path = new Uri("/api/v1/auth/logout", UriKind.Relative);

        (await LogoutClient().PostAsync(path, null).ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        (await LogoutClient().PostAsync(path, null).ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
        (await LogoutClient().PostAsync(path, null).ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        var rejected = await LogoutClient().PostAsync(path, null).ConfigureAwait(true);
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Headers.Should().Contain(h => h.Key == "Retry-After");
        var body = await rejected.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var rejectedDoc = JsonDocument.Parse(body);
        rejectedDoc.RootElement.GetProperty("error").GetString().Should().Be("rate_limited");
        rejectedDoc.RootElement.GetProperty("status").GetInt32().Should().Be(429);

        HttpClient LogoutClient()
        {
            var client = limited.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }

    [Fact]
    public async Task TamperedTokenLogout401()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Admin0003", email: "admin0003@test.local", role: "Admin"))
            .ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var anonymous = _sqlFactory.CreateClient();
        var login = await anonymous.PostAsJsonAsync(
            new Uri("/api/v1/auth/login", UriKind.Relative),
            new LoginRequestDto("Admin0003", TestDataFactory.Password)).ConfigureAwait(true);
        var payload = await login.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(payload);
        var token = doc.RootElement.GetProperty("strToken").GetString()
            ?? throw new InvalidOperationException("Missing token.");
        var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        var client = _sqlFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tampered);
        var response = await client.PostAsync(
            new Uri("/api/v1/auth/logout", UriKind.Relative), null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void AuthActionsCarryExplicitAuthAndRate()
    {
        var controller = typeof(AuthController);

        var login = controller.GetMethod("Login");
        Assert.NotNull(login);
        login.GetCustomAttributes<AllowAnonymousAttribute>().Should().HaveCount(1);
        login.GetCustomAttributes<EnableRateLimitingAttribute>()
            .Select(static a => a.PolicyName).Should().Contain("Login");

        var logout = controller.GetMethod("Logout");
        Assert.NotNull(logout);
        logout.GetCustomAttributes<AuthorizeAttribute>().Should().HaveCount(1);
        logout.GetCustomAttributes<AllowAnonymousAttribute>().Should().BeEmpty();
        logout.GetCustomAttributes<EnableRateLimitingAttribute>()
            .Select(static a => a.PolicyName).Should().Contain("Global");
    }

    [Fact]
    public void JwtValidationIsStrictHs256WithZeroSkew()
    {
        var parameters = _sqlFactory.Services
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;

        parameters.ValidAlgorithms.Should().Equal("HS256");
        parameters.ClockSkew.Should().Be(TimeSpan.Zero);
        parameters.ValidateIssuer.Should().BeTrue();
        parameters.ValidateAudience.Should().BeTrue();
        parameters.ValidateLifetime.Should().BeTrue();
    }

    [Fact]
    public void SecretsScrubbedFromLogEvents()
    {
        var enricher = new SecretScrubbingEnricher();
        ILogEventPropertyFactory factory = new TestPropertyFactory();
        var parser = new Serilog.Parsing.MessageTemplateParser();

        foreach (var name in new[] { "strPWD", "strToken", "password" })
        {
            var log = new LogEvent(
                DateTimeOffset.UtcNow,
                LogEventLevel.Information,
                null,
                parser.Parse("request"),
                [new LogEventProperty(name, new ScalarValue(TestDataFactory.Password))]);

            enricher.Enrich(log, factory);

            log.Properties[name].ToString().Should().Contain("***");
            log.Properties[name].ToString().Should().NotContain(TestDataFactory.Password);
        }
    }

    private sealed class TestPropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false) =>
            new(name, new ScalarValue(value));
    }
}
