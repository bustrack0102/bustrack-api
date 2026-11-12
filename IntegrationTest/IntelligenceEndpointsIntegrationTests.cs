using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>
/// Etapa 3 Integration: Pipeline completo ML (EP-ML-01..07 + EP-AD-08) y GenAI Grounded (EP-IA-01..03).
/// </summary>
public sealed class IntelligenceEndpointsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntelligenceEndpointsIntegrationTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
        });
    }

    [Fact]
    public async Task FlujoMlCompletoConsentimientoLoteCandidatoYAprobacionAdmin()
    {
        using var userClient = _factory.CreateClient();
        var rutasResp = await userClient.GetAsync(new Uri("/api/v1/rutas", UriKind.Relative)).ConfigureAwait(true);
        using var rutasDoc = JsonDocument.Parse(await rutasResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var rutaId = rutasDoc.RootElement.GetProperty("items")[0].GetProperty("Id").GetGuid();

        var token = await RegisterAsync(userClient).ConfigureAwait(true);
        userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var stopSamples = Enumerable.Range(0, 6)
            .Select(i => new { timestampRelativoMs = i * 100, accelX = 0.02, accelY = 0.01, accelZ = 9.81 })
            .ToArray();

        // 1. Sin consentimiento -> 403 Forbidden
        var deniedBatch = await userClient.PostAsJsonAsync(new Uri("/api/v1/sensores/lotes", UriKind.Relative), new
        {
            viajeId = "VJ-ML-001",
            rutaId,
            intervaloMuestreoMs = 100,
            latitudReferencia = 20.0661,
            longitudReferencia = -99.3492,
            nombreParadaSugerido = "Parada Crucero UTTT",
            muestras = stopSamples,
        }).ConfigureAwait(true);
        deniedBatch.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 2. Otorgar consentimiento -> 200
        var grantResp = await userClient.PostAsJsonAsync(new Uri("/api/v1/consentimientos", UriKind.Relative), new
        {
            aceptaAviso = true,
            versionAviso = "1.0",
        }).ConfigureAwait(true);
        grantResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var meConsent = await userClient.GetAsync(new Uri("/api/v1/consentimientos/me", UriKind.Relative)).ConfigureAwait(true);
        meConsent.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Enviar lote inercial -> 201 + genera CandidatoParadaId
        var batchResp = await userClient.PostAsJsonAsync(new Uri("/api/v1/sensores/lotes", UriKind.Relative), new
        {
            viajeId = "VJ-ML-001",
            rutaId,
            intervaloMuestreoMs = 100,
            latitudReferencia = 20.0661,
            longitudReferencia = -99.3492,
            nombreParadaSugerido = "Parada Crucero UTTT",
            muestras = stopSamples,
        }).ConfigureAwait(true);
        batchResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var batchDoc = JsonDocument.Parse(await batchResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        batchDoc.RootElement.GetProperty("ClasificacionPrincipal").GetString().Should().Be("PARADA_DETECTADA");
        var candidatoId = batchDoc.RootElement.GetProperty("CandidatoParadaId").GetString()!;
        candidatoId.Should().NotBeNullOrWhiteSpace();

        // 4. Admin lista lotes y candidatos -> Aprueba candidato -> Promueve a /api/v1/paradas
        using var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());

        var listLotes = await adminClient.GetAsync(new Uri("/api/v1/sensores/lotes", UriKind.Relative)).ConfigureAwait(true);
        listLotes.StatusCode.Should().Be(HttpStatusCode.OK);

        var listCand = await adminClient.GetAsync(new Uri("/api/v1/candidatos-parada?estado=Pendiente", UriKind.Relative)).ConfigureAwait(true);
        listCand.StatusCode.Should().Be(HttpStatusCode.OK);
        (await listCand.Content.ReadAsStringAsync().ConfigureAwait(true)).Should().Contain(candidatoId);

        var validarResp = await adminClient.PatchAsJsonAsync(
            new Uri($"/api/v1/admin/candidatos-parada/{candidatoId}/validar", UriKind.Relative),
            new { aprobar = true, nombreParadaFinal = "Parada Oficial Crucero UTTT" }).ConfigureAwait(true);
        validarResp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var valDoc = JsonDocument.Parse(await validarResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var promotedId = valDoc.RootElement.GetProperty("ParadaPromovidaId").GetString()!;

        var promotedParada = await userClient.GetAsync(new Uri($"/api/v1/paradas/{promotedId}", UriKind.Relative)).ConfigureAwait(true);
        promotedParada.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Revocar consentimiento -> vuelve a bloquear envíos con 403
        var revokeResp = await userClient.DeleteAsync(new Uri("/api/v1/consentimientos/me", UriKind.Relative)).ConfigureAwait(true);
        revokeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var blockedAgain = await userClient.PostAsJsonAsync(new Uri("/api/v1/sensores/lotes", UriKind.Relative), new
        {
            viajeId = "VJ-ML-002",
            rutaId,
            muestras = stopSamples,
        }).ConfigureAwait(true);
        blockedAgain.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FlujoGenAiRecomendacionHistorialYRetroalimentacion()
    {
        using var client = _factory.CreateClient();
        var token = await RegisterAsync(client).ConfigureAwait(true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var recResp = await client.PostAsJsonAsync(new Uri("/api/v1/recomendaciones", UriKind.Relative), new
        {
            origen = "Tula",
            destino = "Tepeji",
            preferencia = "MENOR_COSTO",
            preguntaLibre = "¿Cuál opción cuesta menos?",
        }).ConfigureAwait(true);
        recResp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var recDoc = JsonDocument.Parse(await recResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var recId = recDoc.RootElement.GetProperty("RecomendacionId").GetString()!;

        var histResp = await client.GetAsync(new Uri("/api/v1/recomendaciones/historial", UriKind.Relative)).ConfigureAwait(true);
        histResp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await histResp.Content.ReadAsStringAsync().ConfigureAwait(true)).Should().Contain(recId);

        var fbResp = await client.PostAsJsonAsync(
            new Uri($"/api/v1/recomendaciones/{recId}/retroalimentacion", UriKind.Relative),
            new { util = true, puntuacion = 5, comentario = "Muy clara y coincide con tarifa real." }).ConfigureAwait(true);
        fbResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<string> RegisterAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Colaborador ML",
            correo = $"ml.{Guid.NewGuid():N}@uttt.edu.mx",
            password = "PasswordSegura123!",
            confirmacionPassword = "PasswordSegura123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(true));
        return doc.RootElement.GetProperty("accessToken").GetString()!;
    }
}
