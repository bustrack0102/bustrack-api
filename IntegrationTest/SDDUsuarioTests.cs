using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ApiSDD.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>nv-04 Integration: full cycle plus auth and validation edges on real SQL.</summary>
[Collection("sddsql")]
public sealed class SDDUsuarioTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public SDDUsuarioTests(ApiFactory factory, MsSqlFixture sql)
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

    private HttpClient AdminClient()
    {
        var client = _sqlFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        return client;
    }

    private HttpClient UserClient()
    {
        var client = _sqlFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.UserToken());
        return client;
    }

    private static async Task<JsonDocument> ReadAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        return JsonDocument.Parse(body);
    }

    [Fact]
    public async Task FullCycle201GetPutStale409Delete204Get404()
    {
        var admin = AdminClient();
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createdDoc = await ReadAsync(created).ConfigureAwait(true);
        var id = createdDoc.RootElement.GetProperty("id").GetInt32();
        var version = createdDoc.RootElement.GetProperty("RowVersion").GetString()
            ?? throw new InvalidOperationException("Missing RowVersion.");
        version.Should().NotBeNullOrEmpty();

        var got = await admin.GetAsync(new Uri($"/api/v1/sddusuarios/{id}", UriKind.Relative)).ConfigureAwait(true);
        got.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await admin.PutAsJsonAsync(
            new Uri($"/api/v1/sddusuarios/{id}", UriKind.Relative),
            TestDataFactory.NewUpdate(email: "changed@test.local", rowVersion: version)).ConfigureAwait(true);
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        using var updatedDoc = await ReadAsync(updated).ConfigureAwait(true);
        var fresh = updatedDoc.RootElement.GetProperty("RowVersion").GetString()
            ?? throw new InvalidOperationException("Missing RowVersion.");
        fresh.Should().NotBe(version);

        var stale = await admin.PutAsJsonAsync(
            new Uri($"/api/v1/sddusuarios/{id}", UriKind.Relative),
            TestDataFactory.NewUpdate(email: "stale@test.local", rowVersion: version)).ConfigureAwait(true);
        stale.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var deleted = await admin.DeleteAsync(
            new Uri($"/api/v1/sddusuarios/{id}?rowVersion={Uri.EscapeDataString(fresh)}", UriKind.Relative))
            .ConfigureAwait(true);
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var missing = await admin.GetAsync(new Uri($"/api/v1/sddusuarios/{id}", UriKind.Relative)).ConfigureAwait(true);
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DuplicateUserName409()
    {
        var admin = AdminClient();
        var first = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(email: "other@test.local")).ConfigureAwait(true);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DuplicateEmail409()
    {
        var admin = AdminClient();
        var first = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Usuario02")).ConfigureAwait(true);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MissingToken401()
    {
        var anonymous = _sqlFactory.CreateClient();
        var response = await anonymous.GetAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task NonAdmin403()
    {
        var user = UserClient();
        var response = await user.GetAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task BadPayload422()
    {
        var admin = AdminClient();
        var response = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "short")).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task MalformedRowVersionDelete400()
    {
        var admin = AdminClient();
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = await ReadAsync(created).ConfigureAwait(true);
        var id = doc.RootElement.GetProperty("id").GetInt32();

        var deleted = await admin.DeleteAsync(
            new Uri($"/api/v1/sddusuarios/{id}?rowVersion=!!!", UriKind.Relative)).ConfigureAwait(true);

        deleted.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PagedListReturnsEnvelope()
    {
        var admin = AdminClient();
        foreach (var name in new[] { "Usuario11", "Usuario12", "Usuario13" })
        {
            var response = await admin.PostAsJsonAsync(
                new Uri("/api/v1/sddusuarios", UriKind.Relative),
                TestDataFactory.NewCreate(userName: name, email: $"{name}@test.local")).ConfigureAwait(true);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var list = await admin.GetAsync(
            new Uri("/api/v1/sddusuarios?page=1&pageSize=2", UriKind.Relative)).ConfigureAwait(true);
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = await ReadAsync(list).ConfigureAwait(true);
        doc.RootElement.GetProperty("TotalCount").GetInt32().Should().Be(3);
        doc.RootElement.GetProperty("Page").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("PageSize").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("Items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task StoredDateImmutable()
    {
        var admin = AdminClient();
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        using var createdDoc = await ReadAsync(created).ConfigureAwait(true);
        var id = createdDoc.RootElement.GetProperty("id").GetInt32();
        var version = createdDoc.RootElement.GetProperty("RowVersion").GetString()
            ?? throw new InvalidOperationException("Missing RowVersion.");
        var storedWire = createdDoc.RootElement.GetProperty("dteFechaRegistro").GetString()
            ?? throw new InvalidOperationException("Missing stored date.");
        storedWire.Should().NotBeNullOrEmpty();

        var updated = await admin.PutAsJsonAsync(
            new Uri($"/api/v1/sddusuarios/{id}", UriKind.Relative),
            TestDataFactory.NewUpdate(email: "changed@test.local", rowVersion: version)).ConfigureAwait(true);
        using var updatedDoc = await ReadAsync(updated).ConfigureAwait(true);
        var currentWire = updatedDoc.RootElement.GetProperty("dteFechaRegistro").GetString()
            ?? throw new InvalidOperationException("Missing current date.");
        DateTime.Parse(currentWire, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).Should()
            .Be(DateTime.Parse(storedWire, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
    }

    [Fact]
    public async Task RoleAssignmentRequiresAdmin()
    {
        var user = UserClient();
        var denied = await user.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Usuario21", email: "usuario21@test.local", role: "Admin"))
            .ConfigureAwait(true);
        denied.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var admin = AdminClient();
        var allowed = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(userName: "Usuario22", email: "usuario22@test.local", role: "Admin"))
            .ConfigureAwait(true);
        allowed.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = await ReadAsync(allowed).ConfigureAwait(true);
        doc.RootElement.GetProperty("strRol").GetString().Should().Be("Admin");
    }

    [Fact]
    public async Task EmailNormalizedLowercase()
    {
        var admin = AdminClient();
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate(email: "MiXed@TEST.Local")).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = await ReadAsync(created).ConfigureAwait(true);
        doc.RootElement.GetProperty("strCorreoElectronico").GetString().Should().Be("mixed@test.local");
    }
}
