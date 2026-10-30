using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using ApiSDD.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Common;

namespace ContractTest;

/// <summary>nv-04 Contract: stable wire shape without Pact (dynamic values excluded).</summary>
[Collection("sddsqlcontract")]
public sealed class SDDUsuarioContractTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly MsSqlFixture _sql;
    private WebApplicationFactory<Program> _sqlFactory = null!;

    public SDDUsuarioContractTests(ApiFactory factory, MsSqlFixture sql)
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

    private static string FixturePath()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 6; i++)
        {
            var candidate = Path.Combine(dir, "ContractTest", "Fixtures", "sddusuario.v1.json");
            if (File.Exists(candidate) || Directory.Exists(Path.Combine(dir, "ContractTest")))
            {
                return candidate;
            }

            var parent = Directory.GetParent(dir);
            if (parent is null)
            {
                break;
            }

            dir = parent.FullName;
        }

        throw new FileNotFoundException("ContractTest/Fixtures no encontrado.");
    }

    private static string[] SortedKeys(JsonObject node) =>
        node.Select(static kv => kv.Key).OrderBy(static key => key, StringComparer.Ordinal).ToArray();

    [Fact]
    public async Task LegacyShapeStable()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var listed = await admin.GetAsync(
            new Uri("/api/v1/sddusuarios?page=1&pageSize=20", UriKind.Relative)).ConfigureAwait(true);
        listed.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await listed.Content.ReadAsStringAsync().ConfigureAwait(true);
        var envelope = (JsonNode.Parse(body) ?? throw new InvalidOperationException("Empty list.")).AsObject();
        var items = (envelope["Items"] ?? throw new InvalidOperationException("Missing Items.")).AsArray();
        var item = (items.Single() ?? throw new InvalidOperationException("Empty items.")).AsObject();

        var actual = new JsonObject
        {
            ["envelope"] = new JsonArray(SortedKeys(envelope).Select(static key => (JsonNode)key).ToArray()),
            ["item"] = new JsonArray(SortedKeys(item).Select(static key => (JsonNode)key).ToArray()),
        };

        var path = FixturePath();
        if (string.Equals(
                Environment.GetEnvironmentVariable("CONTRACT_CAPTURE"), "1", StringComparison.Ordinal))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(
                path,
                actual.ToJsonString(new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(true);
            return;
        }

        File.Exists(path).Should().BeTrue("ejecuta CONTRACT_CAPTURE=1 para generar la captura base");
        var expected = JsonNode.Parse(await File.ReadAllTextAsync(path).ConfigureAwait(true));
        JsonNode.DeepEquals(actual, expected).Should().BeTrue();
    }

    [Fact]
    public async Task RowVersionIsEightByteBase64()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        var body = await created.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);

        var wire = doc.RootElement.GetProperty("RowVersion").GetString()
            ?? throw new InvalidOperationException("Missing RowVersion.");
        wire.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(wire).Length.Should().Be(8);
    }

    [Fact]
    public async Task NoSecretKeysOnWire()
    {
        var admin = _sqlFactory.CreateClient();
        admin.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());
        var created = await admin.PostAsJsonAsync(
            new Uri("/api/v1/sddusuarios", UriKind.Relative),
            TestDataFactory.NewCreate()).ConfigureAwait(true);
        var body = await created.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            property.Name.Should().NotBe("strPWD");
            property.Name.Should().NotBe("Status");
        }

        body.Should().NotContain(TestDataFactory.Password);
    }
}
