using FluentAssertions;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>Auditoría de CORS estricto: rechaza orígenes maliciosos y evita wildcard con credenciales.</summary>
public sealed class CorsSecurityTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CorsSecurityTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CorsRejectsUntrustedOriginAndNeverCombinesWildcardWithCredentials()
    {
        using var req = new HttpRequestMessage(HttpMethod.Options, "/api/v1/ping");
        req.Headers.Add("Origin", "https://evil-attacker.example");
        req.Headers.Add("Access-Control-Request-Method", "GET");

        var res = await _client.SendAsync(req).ConfigureAwait(true);

        if (res.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins))
        {
            origins.Should().NotContain("https://evil-attacker.example");
            if (res.Headers.TryGetValues("Access-Control-Allow-Credentials", out var creds) &&
                creds.Contains("true", StringComparer.OrdinalIgnoreCase))
            {
                origins.Should().NotContain("*");
            }
        }
    }
}
