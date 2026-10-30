using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using UnitTest.Common;

namespace ContractTest;

/// <summary>Captura sin Pact real: CONTRACT_CAPTURE=1 regenera, sin var solo valida + convención legacy.</summary>
public sealed class PingFixtureTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public PingFixtureTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static string FixturePath()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 6; i++)
        {
            var candidate = Path.Combine(dir, "ContractTest", "Fixtures", "ping.v1.json");
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

    [Fact]
    public async Task CapturaPingEstable()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/ping").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        var actual = JsonNode.Parse(body);
        actual.Should().NotBeNull();

        var path = FixturePath();
        if (string.Equals(
                Environment.GetEnvironmentVariable("CONTRACT_CAPTURE"), "1",
                StringComparison.Ordinal))
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
    public async Task ConvencionLegacySinPascalCase()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/ping").ConfigureAwait(true);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("Status", out _).Should().BeFalse();
        doc.RootElement.GetProperty("status").GetString().Should().Be("Pong");
    }
}
