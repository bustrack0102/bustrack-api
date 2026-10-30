using System.Net;
using System.Text.Json;
using FluentAssertions;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>NV-01 Integration/Errors 8 en Staging + Detail/Prod + sonda deshabilitada.</summary>
public sealed class ErrorsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ErrorsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static async Task<JsonDocument> ReadAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        return JsonDocument.Parse(body);
    }

    [Fact]
    public async Task RutaInexistente404()
    {
        var client = _factory.CreateStagingClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/no-existe").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SondaProhibida403()
    {
        var client = _factory.CreateStagingClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/forbidden").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var doc = await ReadAsync(response).ConfigureAwait(true);
        doc.RootElement.GetProperty("error").GetString().Should().Be("forbidden");
    }

    [Fact]
    public async Task SondaConflicto409()
    {
        var client = _factory.CreateStagingClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/conflict").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var doc = await ReadAsync(response).ConfigureAwait(true);
        doc.RootElement.GetProperty("error").GetString().Should().Be("conflict");
    }

    [Fact]
    public async Task SondaInvalida422()
    {
        var client = _factory.CreateStagingClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/invalid").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        using var doc = await ReadAsync(response).ConfigureAwait(true);
        doc.RootElement.GetProperty("error").GetString().Should().Be("validation");
    }

    [Fact]
    public async Task SondaTimeout408()
    {
        var client = _factory.CreateStagingClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/timeout").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
    }

    [Fact]
    public async Task SondaError500ConDetailEnStaging()
    {
        var client = _factory.CreateStagingClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/error").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var doc = await ReadAsync(response).ConfigureAwait(true);
        doc.RootElement.GetProperty("error").GetString().Should().Be("internal");
        doc.RootElement.GetProperty("detail").GetString().Should().Contain("Probe failure");
    }

    [Fact]
    public async Task ReadyProdSinTrazas()
    {
        var client = _factory.CreateProductionClient();
        var response = await TestHttp.GetAsync(client, "/health/ready").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        body.Should().NotContain("exception");
        body.Should().NotContain("stack");
        body.Should().NotContain("detail");
        body.Should().Contain("status");
    }

    [Fact]
    public async Task SondaDeshabilitadaEnProd404()
    {
        var client = _factory.CreateProductionClient();
        var response = await TestHttp.GetAsync(client, "/api/v1/probe/timeout").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
