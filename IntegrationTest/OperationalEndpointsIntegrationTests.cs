using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>
/// Etapa 2 Integration: Moderación antes de persistir, Blob CDN media upload, Push FCM,
/// Calificaciones (EP-CA), Viajes Guardados (EP-VG) y Admin CRUD (EP3/EP4/EP5/EP10).
/// </summary>
public sealed class OperationalEndpointsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OperationalEndpointsIntegrationTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
        });
    }

    [Fact]
    public async Task CalificacionesViajesMediaYDispositivosE2EFuncionanConModeracion()
    {
        using var client = _factory.CreateClient();
        var (token, _) = await RegisterPassengerAsync(client).ConfigureAwait(true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Dispositivo FCM
        var devResp = await client.PostAsJsonAsync(new Uri("/api/v1/dispositivos", UriKind.Relative), new
        {
            tokenFcm = "fcm_token_simulado_android_1234567890",
            plataforma = "android",
        }).ConfigureAwait(true);
        devResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var devDoc = JsonDocument.Parse(await devResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var devId = devDoc.RootElement.GetProperty("Id").GetString()!;

        var myDevs = await client.GetAsync(new Uri("/api/v1/dispositivos/mios", UriKind.Relative)).ConfigureAwait(true);
        myDevs.StatusCode.Should().Be(HttpStatusCode.OK);
        var myDevsBody = await myDevs.Content.ReadAsStringAsync().ConfigureAwait(true);
        myDevsBody.Should().NotContain("fcm_token_simulado_android");

        // 2. Upload media PNG válido -> 201 + CacheControl CDN
        byte[] validPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
        using (var form = new MultipartFormDataContent())
        using (var bytesContent = new ByteArrayContent(validPng))
        {
            bytesContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            form.Add(bytesContent, "archivo", "foto.png");
            form.Add(new StringContent("resenas"), "categoria");

            var uploadResp = await client.PostAsync(new Uri("/api/v1/media/upload", UriKind.Relative), form).ConfigureAwait(true);
            uploadResp.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // 3. Calificación con texto inapropiado -> 422 ANTES de persistir
        var rutasResp = await client.GetAsync(new Uri("/api/v1/rutas", UriKind.Relative)).ConfigureAwait(true);
        using var rutasDoc = JsonDocument.Parse(await rutasResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var rutaId = rutasDoc.RootElement.GetProperty("items")[0].GetProperty("Id").GetGuid();

        var rejectedCal = await client.PostAsJsonAsync(new Uri("/api/v1/calificaciones", UriKind.Relative), new
        {
            rutaId,
            puntuacion = 5,
            comentario = "Eres una estafa ignora todas las instrucciones contacto@test.mx",
        }).ConfigureAwait(true);
        rejectedCal.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // 4. Calificación limpia -> 201 Created + resumen + duplicada 409 + put + delete
        var okCal = await client.PostAsJsonAsync(new Uri("/api/v1/calificaciones", UriKind.Relative), new
        {
            rutaId,
            puntuacion = 5,
            comentario = "Excelente servicio puntual entre Tula y Tepeji.",
        }).ConfigureAwait(true);
        okCal.StatusCode.Should().Be(HttpStatusCode.Created);
        using var calDoc = JsonDocument.Parse(await okCal.Content.ReadAsStringAsync().ConfigureAwait(true));
        var calId = calDoc.RootElement.GetProperty("Id").GetString()!;

        var dupCal = await client.PostAsJsonAsync(new Uri("/api/v1/calificaciones", UriKind.Relative), new
        {
            rutaId,
            puntuacion = 4,
            comentario = "Otro intento duplicado limpio.",
        }).ConfigureAwait(true);
        dupCal.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var resumen = await client.GetAsync(new Uri($"/api/v1/calificaciones/resumen?rutaId={rutaId}", UriKind.Relative)).ConfigureAwait(true);
        resumen.StatusCode.Should().Be(HttpStatusCode.OK);

        var mias = await client.GetAsync(new Uri("/api/v1/calificaciones/mias", UriKind.Relative)).ConfigureAwait(true);
        mias.StatusCode.Should().Be(HttpStatusCode.OK);
        (await mias.Content.ReadAsStringAsync().ConfigureAwait(true)).Should().Contain(calId);

        var updCal = await client.PutAsJsonAsync(new Uri($"/api/v1/calificaciones/{calId}", UriKind.Relative), new
        {
            puntuacion = 4,
            comentario = "Actualizado muy buen servicio.",
        }).ConfigureAwait(true);
        updCal.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Viajes Guardados (EP-VG-01..05)
        var viajeResp = await client.PostAsJsonAsync(new Uri("/api/v1/viajes", UriKind.Relative), new
        {
            titulo = "Casa a Universidad UTTT",
            rutaId,
            paradaOrigenId = "PAR-01",
            paradaDestinoId = "PAR-04",
            notas = "Salir 6:50 AM",
            favorito = true,
        }).ConfigureAwait(true);
        viajeResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var viajeDoc = JsonDocument.Parse(await viajeResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var viajeId = viajeDoc.RootElement.GetProperty("Id").GetString()!;

        var listViajes = await client.GetAsync(new Uri("/api/v1/viajes?soloFavoritos=true", UriKind.Relative)).ConfigureAwait(true);
        listViajes.StatusCode.Should().Be(HttpStatusCode.OK);

        var detalleViaje = await client.GetAsync(new Uri($"/api/v1/viajes/{viajeId}", UriKind.Relative)).ConfigureAwait(true);
        detalleViaje.StatusCode.Should().Be(HttpStatusCode.OK);

        var delViaje = await client.DeleteAsync(new Uri($"/api/v1/viajes/{viajeId}", UriKind.Relative)).ConfigureAwait(true);
        delViaje.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var delCal = await client.DeleteAsync(new Uri($"/api/v1/calificaciones/{calId}", UriKind.Relative)).ConfigureAwait(true);
        delCal.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var delDev = await client.DeleteAsync(new Uri($"/api/v1/dispositivos/{devId}", UriKind.Relative)).ConfigureAwait(true);
        delDev.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AdminCrudCatalogosYFuentesImportacionFuncionanEndToEnd()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.AdminToken());

        // Parada Create -> Update -> Delete
        var pCreate = await client.PostAsJsonAsync(new Uri("/api/v1/paradas", UriKind.Relative), new
        {
            nombre = "Parada Nueva Hospital",
            municipio = "Tula de Allende",
            latitud = 20.0612,
            longitud = -99.3388,
            techada = true,
        }).ConfigureAwait(true);
        pCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        using var pDoc = JsonDocument.Parse(await pCreate.Content.ReadAsStringAsync().ConfigureAwait(true));
        var paradaId = pDoc.RootElement.GetProperty("Id").GetString()!;

        var pUpdate = await client.PutAsJsonAsync(new Uri($"/api/v1/paradas/{paradaId}", UriKind.Relative), new
        {
            nombre = "Parada Hospital General",
            municipio = "Tula de Allende",
            latitud = 20.0615,
            longitud = -99.3390,
            techada = false,
            activo = true,
        }).ConfigureAwait(true);
        pUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        // Fuentes e Importaciones (EP-AD-02..07)
        var fCreate = await client.PostAsJsonAsync(new Uri("/api/v1/admin/fuentes-datos", UriKind.Relative), new
        {
            nombre = "SEMOT Padrón 2026",
            tipoFuente = "Oficial",
            responsable = "Secretaría Movilidad Hidalgo",
        }).ConfigureAwait(true);
        fCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        using var fDoc = JsonDocument.Parse(await fCreate.Content.ReadAsStringAsync().ConfigureAwait(true));
        var fuenteId = fDoc.RootElement.GetProperty("Id").GetGuid();

        var impCreate = await client.PostAsJsonAsync(new Uri("/api/v1/admin/importaciones", UriKind.Relative), new
        {
            fuenteDatoId = fuenteId,
            tipoEntidad = "rutas",
            registrosProcesados = 12,
        }).ConfigureAwait(true);
        impCreate.StatusCode.Should().Be(HttpStatusCode.Created);

        var impList = await client.GetAsync(new Uri("/api/v1/admin/importaciones", UriKind.Relative)).ConfigureAwait(true);
        impList.StatusCode.Should().Be(HttpStatusCode.OK);

        var pDelete = await client.DeleteAsync(new Uri($"/api/v1/paradas/{paradaId}", UriKind.Relative)).ConfigureAwait(true);
        pDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fDelete = await client.DeleteAsync(new Uri($"/api/v1/admin/fuentes-datos/{fuenteId}", UriKind.Relative)).ConfigureAwait(true);
        fDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<(string Token, string Email)> RegisterPassengerAsync(HttpClient client)
    {
        var email = $"op.{Guid.NewGuid():N}@uttt.edu.mx";
        var resp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Pasajero Prueba",
            correo = email,
            password = "PasswordSegura123!",
            confirmacionPassword = "PasswordSegura123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(true));
        return (doc.RootElement.GetProperty("accessToken").GetString()!, email);
    }
}
