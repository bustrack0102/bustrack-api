using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace SecurityTest;

/// <summary>
/// Etapa 2 Security: Protección IDOR en Viajes/Calificaciones, RBAC Admin en CRUD catálogos/fuentes y rechazo de MIME falsificado.
/// </summary>
public sealed class OperationalSecurityTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OperationalSecurityTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
        });
    }

    [Fact]
    public async Task UsuarioNoPuedeLeerNiEliminarViajeDeOtroUsuarioIdor403()
    {
        using var client = _factory.CreateClient();
        var rutasResp = await client.GetAsync(new Uri("/api/v1/rutas", UriKind.Relative)).ConfigureAwait(true);
        using var rutasDoc = JsonDocument.Parse(await rutasResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var rutaId = rutasDoc.RootElement.GetProperty("items")[0].GetProperty("Id").GetGuid();

        var tokenA = await RegisterAndGetTokenAsync(client).ConfigureAwait(true);
        var tokenB = await RegisterAndGetTokenAsync(client).ConfigureAwait(true);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var createResp = await client.PostAsJsonAsync(new Uri("/api/v1/viajes", UriKind.Relative), new
        {
            titulo = "Mi Viaje Privado",
            rutaId,
            favorito = true,
        }).ConfigureAwait(true);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var viajeId = doc.RootElement.GetProperty("Id").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var getOther = await client.GetAsync(new Uri($"/api/v1/viajes/{viajeId}", UriKind.Relative)).ConfigureAwait(true);
        var delOther = await client.DeleteAsync(new Uri($"/api/v1/viajes/{viajeId}", UriKind.Relative)).ConfigureAwait(true);

        getOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        delOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UsuarioComunRecibe403EnEndpointsAdminY422AnteMimeFalsificadoEnMedia()
    {
        using var client = _factory.CreateClient();
        var userToken = await RegisterAndGetTokenAsync(client).ConfigureAwait(true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var paradaDenied = await client.PostAsJsonAsync(new Uri("/api/v1/paradas", UriKind.Relative), new
        {
            nombre = "Parada Ilegal",
            municipio = "Tula",
            latitud = 20.05,
            longitud = -99.34,
        }).ConfigureAwait(true);
        paradaDenied.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var fuenteDenied = await client.GetAsync(new Uri("/api/v1/admin/fuentes-datos", UriKind.Relative)).ConfigureAwait(true);
        fuenteDenied.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        byte[] disguisedHtml = "<!DOCTYPE html><html><body>fake-image-header</body></html>"u8.ToArray();
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(disguisedHtml);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(fileContent, "archivo", "simulado.jpg");

        var mediaResp = await client.PostAsync(new Uri("/api/v1/media/upload", UriKind.Relative), form).ConfigureAwait(true);
        mediaResp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private static async Task<string> RegisterAndGetTokenAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Usuario Seguridad",
            correo = $"sec.{Guid.NewGuid():N}@uttt.edu.mx",
            password = "PasswordSegura123!",
            confirmacionPassword = "PasswordSegura123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(true));
        return doc.RootElement.GetProperty("accessToken").GetString()!;
    }
}
