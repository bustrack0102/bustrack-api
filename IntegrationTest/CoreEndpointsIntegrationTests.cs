using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ApiSDD.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>
/// Etapa 1 Integration: ciclo E2E completo sobre persistencia NoSQL + Redis distribuido
/// (EP-AU-01..09, EP-EM-01..05, EP3 Rutas/Paradas/Mapa, EP4 Horarios/Tarifas, EP5 Destinos, EP-AD-01/09).
/// </summary>
public sealed class CoreEndpointsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _mongoFactory;

    public CoreEndpointsIntegrationTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _mongoFactory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
        });
    }

    [Fact]
    public async Task FlujoAuthCompletoRegistroLoginRefreshLogoutYDeleteMe()
    {
        using var client = _mongoFactory.CreateClient();
        var email = $"pasajero.{Guid.NewGuid():N}@uttt.edu.mx";

        // 1. Registro (EP-AU-01)
        var regResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Ana María López",
            correo = email,
            password = "PasswordSegura123!",
            confirmacionPassword = "PasswordSegura123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);

        regResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var regDoc = JsonDocument.Parse(await regResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var accessToken = regDoc.RootElement.GetProperty("accessToken").GetString()!;
        var refreshToken = regDoc.RootElement.GetProperty("refreshToken").GetString()!;

        // 2. Perfil me (EP-AU-07) y Edición (EP-AU-08)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResp = await client.GetAsync(new Uri("/api/v1/usuarios/me", UriKind.Relative)).ConfigureAwait(true);
        meResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var updResp = await client.PutAsJsonAsync(new Uri("/api/v1/usuarios/me", UriKind.Relative), new
        {
            nombreCompleto = "Ana M. López",
            correo = email,
        }).ConfigureAwait(true);
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Refresh con rotación (EP-AU-03)
        var refreshResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/refresh", UriKind.Relative),
            new RefreshTokenRequestDto(refreshToken)).ConfigureAwait(true);
        refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var refDoc = JsonDocument.Parse(await refreshResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var rotatedAccess = refDoc.RootElement.GetProperty("accessToken").GetString()!;
        var rotatedRefresh = refDoc.RootElement.GetProperty("refreshToken").GetString()!;

        // Reintentar refresh viejo debe fallar 401
        var reusedOld = await client.PostAsJsonAsync(new Uri("/api/v1/auth/refresh", UriKind.Relative),
            new RefreshTokenRequestDto(refreshToken)).ConfigureAwait(true);
        reusedOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 4. Logout con revocación en blacklist distribuida (EP-AU-04 + Req 10)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rotatedAccess);
        var logoutResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/logout", UriKind.Relative),
            new LogoutRequestDto(rotatedRefresh, false)).ConfigureAwait(true);
        logoutResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Token revocado no debe seguir accediendo a /usuarios/me
        var revokedMe = await client.GetAsync(new Uri("/api/v1/usuarios/me", UriKind.Relative)).ConfigureAwait(true);
        revokedMe.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 5. Login posterior + Baja definitiva de cuenta (EP-AU-02 + EP-AU-09)
        var loginResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new
        {
            correo = email,
            password = "PasswordSegura123!",
        }).ConfigureAwait(true);
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var loginDoc = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var freshAccess = loginDoc.RootElement.GetProperty("accessToken").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", freshAccess);
        using var delReq = new HttpRequestMessage(HttpMethod.Delete, new Uri("/api/v1/usuarios/me", UriKind.Relative))
        {
            Content = JsonContent.Create(new UsuarioMeDeleteRequestDto("PasswordSegura123!")),
        };
        var delResp = await client.SendAsync(delReq).ConfigureAwait(true);
        delResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cuenta dada de baja no puede volver a iniciar sesión
        var loginAfterDelete = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new
        {
            correo = email,
            password = "PasswordSegura123!",
        }).ConfigureAwait(true);
        loginAfterDelete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/v1/empresas")]
    [InlineData("/api/v1/empresas/autocompletar?q=Valle")]
    [InlineData("/api/v1/empresas/comparar?paradaOrigenId=Tula")]
    [InlineData("/api/v1/empresas/20000000-0000-0000-0000-000000000001")]
    [InlineData("/api/v1/empresas/20000000-0000-0000-0000-000000000001/rutas")]
    [InlineData("/api/v1/medios-transporte")]
    [InlineData("/api/v1/rutas/buscar?destino=Tula")]
    [InlineData("/api/v1/rutas/autocompletar?q=Tepeji")]
    [InlineData("/api/v1/rutas/10000000-0000-0000-0000-000000000001/geometria")]
    [InlineData("/api/v1/rutas/10000000-0000-0000-0000-000000000001/horarios")]
    [InlineData("/api/v1/rutas/10000000-0000-0000-0000-000000000001/tarifas")]
    [InlineData("/api/v1/paradas/autocompletar?q=Central")]
    [InlineData("/api/v1/paradas/PAR-04")]
    [InlineData("/api/v1/mapa/elementos?tipo=todos")]
    [InlineData("/api/v1/horarios?rutaId=10000000-0000-0000-0000-000000000001")]
    [InlineData("/api/v1/horarios/siguiente?rutaId=10000000-0000-0000-0000-000000000001&horaActual=06:15")]
    [InlineData("/api/v1/tarifas")]
    [InlineData("/api/v1/tarifas/calcular?rutaId=10000000-0000-0000-0000-000000000001&tipoPasajero=Estudiante")]
    [InlineData("/api/v1/destinos")]
    [InlineData("/api/v1/destinos/autocompletar?q=Tula")]
    [InlineData("/api/v1/destinos/categorias")]
    [InlineData("/api/v1/destinos/DES-01")]
    [InlineData("/api/v1/destinos/DES-01/rutas")]
    [InlineData("/api/v1/catalogos/estado-actualizacion")]
    public async Task EndpointsCoreMvpNoSqlResponden200ConDatosReales(string path)
    {
        using var client = _mongoFactory.CreateClient();
        var response = await client.GetAsync(new Uri(path, UriKind.Relative)).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        body.Should().NotBeNullOrWhiteSpace();
    }
}
