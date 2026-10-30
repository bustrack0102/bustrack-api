using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using ApiSDD.Controllers.V1;
using ApiSDD.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UnitTest.Common;

namespace SecurityTest;

/// <summary>nv-04 Security: no secret leaks, uniform 429, strict JWT, rate metadata.</summary>
[Collection("sddsqlsec")]
public sealed class SDDUsuarioSecurityTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public SDDUsuarioSecurityTests(ApiFactory factory, MsSqlFixture sql)
    {
        _factory = factory;
        _sql = sql;
    }

    public async Task InitializeAsync()
    {
        _sqlFactory = _factory.WithConnectionString(_sql.ConnectionString);
        using var scope = _sqlFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync().ConfigureAwait(true);
        await db.SDDUsuarios.ExecuteDeleteAsync().ConfigureAwait(true);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task HashAndPlainNeverLeak()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdBody = await created.Content.ReadAsStringAsync().ConfigureAwait(true);

        var listed = await admin.GetAsync(
            new Uri("/api/v1/sddusuarios?page=1&pageSize=20", UriKind.Relative)).ConfigureAwait(true);
        var listedBody = await listed.Content.ReadAsStringAsync().ConfigureAwait(true);

        foreach (var body in new[] { createdBody, listedBody })
        {
            body.Should().NotContain("strPWD");
            body.Should().NotContain(TestDataFactory.Password);
        }

        using var doc = JsonDocument.Parse(listedBody);
        foreach (var item in doc.RootElement.GetProperty("Items").EnumerateArray())
        {
            item.TryGetProperty("strPWD", out _).Should().BeFalse();
            item.TryGetProperty("RowVersion", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task AdminRateLimit429Uniform()
    {
        var limited = _factory.WithWebHostBuilder(
            static builder => builder.UseSetting("RateLimiting:AdminPerMinute", "2"));
        var client = limited.CreateClient();
        var path = new Uri("/api/v1/sddusuarios", UriKind.Relative);

        (await client.GetAsync(path).ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
        (await client.GetAsync(path).ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        var rejected = await client.GetAsync(path).ConfigureAwait(true);
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Headers.Should().Contain(h => h.Key == "Retry-After");
        var body = await rejected.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("error").GetString().Should().Be("rate_limited");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(429);
    }

    [Fact]
    public async Task WrongAlgorithmRejected401()
    {
        var client = _sqlFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTestHelper.CreateToken("Admin", SecurityAlgorithms.HmacSha384));
        var response = await client.GetAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExpiredTokenRejected401()
    {
        var client = _sqlFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTestHelper.ExpiredAdminToken());
        var response = await client.GetAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void WriteEndpointsCarryBothRatePolicies()
    {
        var controller = typeof(SDDUsuariosController);
        controller.GetCustomAttributes<EnableRateLimitingAttribute>()
            .Select(static a => a.PolicyName).Should().Contain("Admin");

        foreach (var action in new[] { "Create", "Update", "Delete" })
        {
            var method = controller.GetMethod(action);
            Assert.NotNull(method);
            method.GetCustomAttributes<EnableRateLimitingAttribute>()
                .Select(static a => a.PolicyName).Should().Contain("ConcurrentWrites");
        }
    }
}
