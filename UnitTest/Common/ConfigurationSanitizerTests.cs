using System.Net;
using System.Net.Http;
using System.Text;
using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;

namespace UnitTest.Common;

/// <summary>
/// Pruebas unitarias de sanitización de secretos y tolerancia ante documentos heredados incompatibles en MongoDB.
/// </summary>
public sealed class ConfigurationSanitizerTests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly string _jsonResponse;

        public CapturingHandler(string jsonResponse) => _jsonResponse = jsonResponse;

        public string? CapturedHeader { get; private set; }
        public Uri? CapturedUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedUri = request.RequestUri;
            if (request.Headers.TryGetValues("X-Internal-Key", out var vals))
            {
                CapturedHeader = vals.FirstOrDefault();
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_jsonResponse, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SingleClientFactory(HttpClient client) => _client = client;

        public HttpClient CreateClient(string name) => _client;
    }

    [Fact]
    public void CleanSecretEliminaSaltosDeLineaYCControlSinAlterarClave()
    {
        var dirty = "\uFEFF  clave-interna-segura\r\n\u200B ";
        var clean = ConfigurationSanitizer.CleanSecret(dirty);
        clean.Should().Be("clave-interna-segura");
        ConfigurationSanitizer.HasUsableValue(clean).Should().BeTrue();
        ConfigurationSanitizer.HasUsableValue("REEMPLAZAR_CON_SECRETO").Should().BeFalse();
    }

    [Fact]
    public async Task ClientesMlYGenAiNoFallanConNewlineEnInternalKey()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:MlBaseUrl"] = "https://ml.internal.test/\r\n",
                ["Services:GenAiBaseUrl"] = "https://genai.internal.test/\n",
                ["InternalService:Key"] = "secreto-con-salto-de-linea\r\n",
            })
            .Build();

        using var mlHandler = new CapturingHandler("""{"inferenciaId":"inf-1","viajeAnonimoHash":"abc","rutaId":"R-01","modeloVersion":"1.0.0","clasificacionPrincipal":"EN_TRAMO","confianzaGlobal":0.95,"severidadMaxima":0,"totalMuestrasProcesadas":2,"procesadoEnMs":12}""");
        using var mlHttp = new HttpClient(mlHandler);
        var mlClient = new MlServiceClient(new SingleClientFactory(mlHttp), config, NullLogger<MlServiceClient>.Instance);
        var mlResult = await mlClient.DetectAsync(
            "VJ-TEST-1",
            "R-01",
            100,
            true,
            [new SensorMuestraDto(0, 0.1, 0.1, 9.8), new SensorMuestraDto(100, 0.1, 0.1, 9.8)],
            CancellationToken.None);

        mlHandler.CapturedHeader.Should().Be("secreto-con-salto-de-linea");
        mlResult.ModeloVersion.Should().Be("1.0.0");

        using var genaiHandler = new CapturingHandler("""{"recomendacionId":"rec-1","rutaRecomendadaId":"R-01","explicacion":"Recomendada R-01.","promptVersion":"v1"}""");
        using var genaiHttp = new HttpClient(genaiHandler);
        var genaiClient = new GenAiServiceClient(new SingleClientFactory(genaiHttp), config, NullLogger<GenAiServiceClient>.Instance);
        var genaiResult = await genaiClient.RecommendAsync(
            new GenAiUpstreamRequest(
                "usr-1",
                "DES-01",
                new GenAiPreferencesDto("rapidez", "es", null),
                [new GenAiRouteCandidateDto("R-01", "Pachuca-Tula", "emp-1", "AVM", "Autobús Directo", "Central", "UTTT", 65.0, 30, 4.6, ["06:00"], ["Central"])]),
            CancellationToken.None);

        genaiHandler.CapturedHeader.Should().Be("secreto-con-salto-de-linea");
        genaiResult.Should().NotBeNull();
        genaiResult.PromptVersion.Should().Be("v1");
    }

    [Fact]
    public void DeserializeCompatibleOmiteDocumentoHeredadoSinGuidYPreservaValidos()
    {
        MongoDbContext.EnsureBsonConventionsRegistered();
        var seedRuta = BusTrackSeedData.Rutas()[0];
        var validBson = seedRuta.ToBsonDocument();
        var legacyInvalidBson = new BsonDocument
        {
            ["_id"] = "RUT-LEGACY-01",
            ["codigo"] = "OLD",
            ["nombre"] = "Ruta heredada pre-UUID",
        };

        var loaded = MongoBusTrackStore.DeserializeCompatible<RutaDoc>([legacyInvalidBson, validBson], "rutas");
        loaded.Should().ContainSingle(r => r.Id == seedRuta.Id);
    }
}
