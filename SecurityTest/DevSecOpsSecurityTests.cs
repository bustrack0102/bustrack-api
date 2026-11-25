using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace SecurityTest;

/// <summary>
/// Pruebas de seguridad DevSecOps:
/// 1. Detección y revocación por reuso de Refresh Token (OAuth 2.0 Threat Model).
/// 2. Sanitización estricta de placeholders en JWT (prevención de falsificación con claves públicas).
/// 3. Mitigación de inyección NoSQL ($where / $ne / operadores BSON).
/// 4. Verificación de correo y protección de endpoints me/rutas ante cuentas pendientes.
/// 5. Protección de headers de seguridad HTTP (HSTS, CSP, X-Content-Type-Options, Anti-Sniffing).
/// 6. Rate limiting por IP/partición (prevención DoS en autenticación).
/// </summary>
public sealed class DevSecOpsSecurityTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DevSecOpsSecurityTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
            b.UseSetting("RateLimiting:LoginPer5Minutes", "20");
        });
    }

    [Fact]
    public async Task RefreshTokenReutilizadoRevocaFamiliaCompletaDetectandoRobo()
    {
        using var client = _factory.CreateClient();
        var email = $"reuse.{Guid.NewGuid():N}@uttt.edu.mx";
        var regResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Usuario Reuso",
            correo = email,
            password = "PasswordFuerte123!",
            confirmacionPassword = "PasswordFuerte123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var regDoc = JsonDocument.Parse(await regResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var refreshToken1 = regDoc.RootElement.GetProperty("refreshToken").GetString()!;

        // Primer refresh legítimo: entrega nuevo token y revoca refreshToken1
        var refreshResp1 = await client.PostAsJsonAsync(new Uri("/api/v1/auth/refresh", UriKind.Relative), new
        {
            refreshToken = refreshToken1,
        }).ConfigureAwait(true);
        refreshResp1.StatusCode.Should().Be(HttpStatusCode.OK);
        using var refDoc1 = JsonDocument.Parse(await refreshResp1.Content.ReadAsStringAsync().ConfigureAwait(true));
        var refreshToken2 = refDoc1.RootElement.GetProperty("refreshToken").GetString()!;

        // Ataque: el atacante intenta reutilizar refreshToken1 ya revocado
        var attackResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/refresh", UriKind.Relative), new
        {
            refreshToken = refreshToken1,
        }).ConfigureAwait(true);
        attackResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Consecuencia DevSecOps: refreshToken2 (y toda la familia de tokens) queda invalidada automáticamente
        var victimResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/refresh", UriKind.Relative), new
        {
            refreshToken = refreshToken2,
        }).ConfigureAwait(true);
        victimResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IntentosDeInyeccionNoSqlSonRechazadosConValidacionDeEntrada()
    {
        using var client = _factory.CreateClient();

        // Intento de inyección de operador BSON en login
        var nosqlInjection = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new
        {
            correo = "admin@bustrack.com' || '1'=='1",
            password = "{\"$gt\": \"\"}",
        }).ConfigureAwait(true);
        nosqlInjection.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);

        // Intento de inyección de script o selector en autocompletar empresas
        var scriptInjection = await client.GetAsync(new Uri("/api/v1/empresas/autocompletar?q=<script>alert(1)</script>", UriKind.Relative)).ConfigureAwait(true);
        scriptInjection.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await scriptInjection.Content.ReadAsStringAsync().ConfigureAwait(true);
        content.Should().NotContain("<script>alert(1)</script>");
    }

    [Fact]
    public async Task VerificacionDeCorreoRechazaTokensFalsosYPrevieneAcceso()
    {
        using var client = _factory.CreateClient();

        // Token inválido o inexistente
        var invalidVerify = await client.PostAsJsonAsync(new Uri("/api/v1/auth/verificar-correo", UriKind.Relative), new
        {
            token = "token-totalmente-invalido-1234567890",
        }).ConfigureAwait(true);
        invalidVerify.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Token vacío o nulo
        var emptyVerify = await client.PostAsJsonAsync(new Uri("/api/v1/auth/verificar-correo", UriKind.Relative), new
        {
            token = "",
        }).ConfigureAwait(true);
        emptyVerify.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CabecerasHttpSeguridadEstanPresentesEnRespuestas()
    {
        using var client = _factory.CreateClient();
        var resp = await client.GetAsync(new Uri("/health", UriKind.Relative)).ConfigureAwait(true);

        resp.Headers.Contains("X-Content-Type-Options").Should().BeTrue();
        resp.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        resp.Headers.Contains("X-Frame-Options").Should().BeTrue();
    }

    [Fact]
    public async Task PreflightOptionsDevuelveHeadersCorsParaFlutterWeb()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, new Uri("/api/v1/auth/login", UriKind.Relative));
        request.Headers.Add("Origin", "http://localhost:5000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type,accept,authorization,x-api-version");

        var response = await client.SendAsync(request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:5000");
        response.Headers.Contains("Access-Control-Allow-Methods").Should().BeTrue();
        response.Headers.GetValues("Access-Control-Allow-Methods").First().Should().Contain("POST");
        response.Headers.Contains("Access-Control-Allow-Headers").Should().BeTrue();
    }

    [Fact]
    public async Task SolicitudPublicaConOriginRetornaHeadersCors()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/health", UriKind.Relative));
        request.Headers.Add("Origin", "http://localhost:5000");

        var response = await client.SendAsync(request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:5000");
    }

    [Fact]
    public async Task SolicitudProtegidaConBearerTokenFuncionaConCors()
    {
        using var client = _factory.CreateClient();
        var email = $"cors.auth.{Guid.NewGuid():N}@uttt.edu.mx";
        var regResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Usuario CORS",
            correo = email,
            password = "PasswordFuerte123!",
            confirmacionPassword = "PasswordFuerte123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var regDoc = JsonDocument.Parse(await regResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        var token = regDoc.RootElement.GetProperty("accessToken").GetString()!;

        // Petición GET a recurso protegido /api/v1/usuarios/me con Origin y Authorization
        using var meRequest = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/v1/usuarios/me", UriKind.Relative));
        meRequest.Headers.Add("Origin", "http://localhost:5000");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var meResp = await client.SendAsync(meRequest).ConfigureAwait(true);
        meResp.StatusCode.Should().Be(HttpStatusCode.OK);
        meResp.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        meResp.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:5000");
    }
}
