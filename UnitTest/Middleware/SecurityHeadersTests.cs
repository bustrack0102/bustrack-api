using ApiSDD.Middleware;
using FluentAssertions;
using FsCheck;
using Microsoft.AspNetCore.Http;

namespace UnitTest.Middleware;

/// <summary>NV-01 Unit/Middleware 10: headers+CSP+nonce 16B único, 5 exentas, set antes _next, 2 nulos.</summary>
public sealed class SecurityHeadersTests
{
    private static DefaultHttpContext Arrange(string path = "/")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public void ConstructorRechazaNextNulo()
    {
        Action act = () => _ = new SecurityHeadersMiddleware(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeRechazaContextoNulo()
    {
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);
        Func<Task> act = () => middleware.InvokeAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().ConfigureAwait(true);
    }

    [Fact]
    public async Task AplicaHeadersYCspConNonce()
    {
        var context = Arrange("/api/v1/ping");
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("0");
        var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
        csp.Should().Contain("nonce-");
        context.Items[SecurityHeadersMiddleware.NonceItemKey].Should().NotBeNull();
    }

    [Fact]
    public void NonceIdaVueltaBase64Determinista()
    {
        // FsCheck determinista: enteros fijos → misma muestra siempre.
        var samples = FsCheck.Fluent.Gen.Sample(
            FsCheck.Fluent.Gen.ArrayOf(FsCheck.Fluent.Gen.Choose(0, 255), 16),
            50,
            42).ToList();
        samples.Should().HaveCount(50);
        var nonces = new List<string>();
        foreach (var sample in samples)
        {
            var bytes = sample.Select(i => (byte)i).ToArray();
            var nonce = SecurityHeadersMiddleware.CreateNonce(bytes);
            Convert.FromBase64String(nonce).Should().BeEquivalentTo(bytes);
            nonces.Add(nonce);
        }

        nonces.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData("/scalar")]
    [InlineData("/scalar/v1")]
    [InlineData("/openapi/v1.json")]
    [InlineData("/openapi")]
    [InlineData("/SCALAR")]
    public async Task RutasExentasSinCsp(string path)
    {
        var context = Arrange(path);
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.Headers.ContentSecurityPolicy.ToString().Should().BeEmpty();
        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task HeadersAntesDeNext()
    {
        var context = Arrange("/");
        var seen = new Dictionary<string, bool>();
        var middleware = new SecurityHeadersMiddleware(ctx =>
        {
            seen["nosniff"] = ctx.Response.Headers.ContainsKey("X-Content-Type-Options");
            seen["nonce"] = ctx.Items.ContainsKey(SecurityHeadersMiddleware.NonceItemKey);
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        seen.Should().ContainKey("nosniff").WhoseValue.Should().BeTrue();
        seen.Should().ContainKey("nonce").WhoseValue.Should().BeTrue();
    }
}
