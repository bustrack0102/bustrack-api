using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace UnitTest.Common;

/// <summary>NV-00 T1: Example.json válido, sin secretos y sin claves futuras.</summary>
public sealed class AppSettingsTests
{
    private static string ExamplePath()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 6; i++)
        {
            var candidate = Path.Combine(dir, "API_SDD", "appsettings.Example.json");
            if (File.Exists(candidate))
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

        throw new FileNotFoundException("API_SDD/appsettings.Example.json no encontrado.");
    }

    private static JsonDocument LoadExample(out string raw)
    {
        raw = File.ReadAllText(ExamplePath());
        return JsonDocument.Parse(raw);
    }

    [Fact]
    public void ExampleTraeClavesUsadas()
    {
        using var doc = LoadExample(out _);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("ConnectionStrings", out var cs)
            && cs.TryGetProperty("Default", out _));
        Assert.True(root.TryGetProperty("UseInMemoryDatabase", out _));
        Assert.True(root.TryGetProperty("Jwt", out var jwt)
            && jwt.TryGetProperty("Key", out _)
            && jwt.TryGetProperty("Issuer", out _)
            && jwt.TryGetProperty("Audience", out _));
        Assert.True(root.TryGetProperty("Cors", out var cors)
            && cors.TryGetProperty("AllowedOrigins", out _));
        Assert.True(root.TryGetProperty("Kestrel", out _));
        Assert.True(root.TryGetProperty("DataProtection", out _));
    }

    [Fact]
    public void ExampleSinPatronesSensiblesReales()
    {
        using var doc = LoadExample(out _);

        // Solo las secciones sensibles deben ser placeholders, nunca valores reales.
        foreach (var section in new[] { "ConnectionStrings", "Jwt", "DataProtection" })
        {
            if (!doc.RootElement.TryGetProperty(section, out var element))
            {
                continue;
            }

            var leaves = new List<string>();
            CollectLeaves(element, leaves);
            leaves.Should().NotBeEmpty();
            leaves.Should().OnlyContain(leaf =>
                leaf.Contains("REEMPLAZAR", StringComparison.Ordinal)
                || leaf.Contains("EXTERNA", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ExampleSinClavesFuturas()
    {
        using var doc = LoadExample(out var raw);

        foreach (var key in new[] { "Sqs", "Transport", "Redis", "PERF_" })
        {
            Assert.DoesNotContain(key, raw, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void CollectLeaves(JsonElement el, List<string> acc)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in el.EnumerateObject())
                {
                    CollectLeaves(prop.Value, acc);
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    CollectLeaves(item, acc);
                }

                break;
            case JsonValueKind.String:
                var s = el.GetString();
                if (!string.IsNullOrEmpty(s))
                {
                    acc.Add(s);
                }

                break;
            case JsonValueKind.Number:
                acc.Add(el.GetRawText());
                break;
            default:
                break;
        }
    }
}
