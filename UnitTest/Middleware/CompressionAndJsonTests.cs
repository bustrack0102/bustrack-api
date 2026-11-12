using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ApiSDD.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Common;

namespace UnitTest.Middleware;

/// <summary>Etapa 0: Compresión Brotli/Gzip, Serialización Source-Gen JSON y Health NoSQL.</summary>
public sealed class CompressionAndJsonTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public CompressionAndJsonTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    [Fact]
    public async Task RespuestaUsaBrotliCuandoClienteLoAcepta()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.AcceptEncoding.Clear();
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        var response = await client.GetAsync(new Uri("/api/v1/rutas", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentEncoding.Should().Contain("br");
    }

    [Fact]
    public async Task ProductionConPersistenceMongoSinCadenaRetorna503SinTrazas()
    {
        using var client = _factory.CreateProductionClient();
        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("status").GetString().Should().Be("Unhealthy");
        doc.RootElement.GetProperty("results").GetProperty("mongodb").GetString().Should().Be("Unhealthy");
    }

    [Fact]
    public void MongoDbContextNoLanzaEnConstructorAntePlaceholders()
    {
        using var scope = _factory.WithWebHostBuilder(static b =>
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_ENV_EXTERNA"))
            .Services.CreateScope();

        var mongo = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        mongo.IsConfigured.Should().BeFalse();
    }
}
