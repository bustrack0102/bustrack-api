using System.Net;
using System.Text.Json;
using FluentAssertions;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>NV-01 Integration/Routing 3: v1+legacy, sin versión 404, openapi/scalar Dev/Prod.</summary>
public sealed class RoutingTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public RoutingTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task VersionResuelveYLegacy()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/ping").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("status").GetString().Should().Be("Pong");
        doc.RootElement.TryGetProperty("Status", out _).Should().BeFalse();
    }

    [Fact]
    public async Task SinVersion404()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/api/ping").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OpenApiYScalarSoloDev()
    {
        var dev = _factory.CreateClient();
        (await TestHttp.GetAsync(dev, "/openapi/v1.json").ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.OK);
        (await TestHttp.GetAsync(dev, "/scalar/v1").ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.OK);

        var prod = _factory.CreateProductionClient();
        (await TestHttp.GetAsync(prod, "/openapi/v1.json").ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
        (await TestHttp.GetAsync(prod, "/scalar/v1").ConfigureAwait(true)).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OpenApiExposesConcreteV1Paths()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/openapi/v1.json").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        body.Should().NotContain("{version}");
        body.Should().Contain("/api/v1/SDDUsuarios");
    }

    [Fact]
    public async Task OpenApiDeclaresBearerScheme()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/openapi/v1.json").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        body.Should().Contain("securitySchemes");
        body.Should().Contain("Bearer");
        body.Should().Contain("bearerFormat");
    }
}
