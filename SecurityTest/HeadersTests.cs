using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UnitTest.Common;

namespace SecurityTest;

/// <summary>NV-01 Security/Headers 3: health anónimo con headers, 403 con headers, HSTS 365d.</summary>
public sealed class HeadersTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HeadersTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthAnonimoConHeadersYCsp()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/health").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
        response.Headers.Should().Contain(h =>
            h.Key == "Content-Security-Policy" && h.Value.Any(v => v.Contains("nonce-", StringComparison.Ordinal)));
        response.Headers.Should().Contain(h => h.Key == "X-Correlation-Id");
    }

    [Fact]
    public async Task ProhibidoConHeaders()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/forbidden").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
        response.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
    }

    [Fact]
    public void HstsMaxAge365DiasEnFactory()
    {
        var options = _factory.Services.GetRequiredService<IOptions<HstsOptions>>().Value;

        options.MaxAge.Should().Be(TimeSpan.FromDays(365));
    }
}
