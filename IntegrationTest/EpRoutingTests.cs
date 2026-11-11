using System.Net;
using FluentAssertions;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>
/// EP-AU/EP-EM Integration/Routing: verifica que las rutas de los controladores
/// de auth V2, usuarios, empresas y medios-transporte resuelven y aplican su
/// política de autorización (401 determinista sin base de datos).
/// </summary>
public sealed class EpRoutingTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public EpRoutingTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("GET", "/api/v1/usuarios")]
    [InlineData("GET", "/api/v1/usuarios/me")]
    [InlineData("PUT", "/api/v1/usuarios/me")]
    [InlineData("DELETE", "/api/v1/usuarios/me")]
    [InlineData("POST", "/api/v1/auth/logout")]
    [InlineData("POST", "/api/v1/empresas")]
    [InlineData("PUT", "/api/v1/empresas/00000000-0000-0000-0000-000000000000")]
    [InlineData("DELETE", "/api/v1/empresas/00000000-0000-0000-0000-000000000000")]
    [InlineData("POST", "/api/v1/medios-transporte")]
    [InlineData("PUT", "/api/v1/medios-transporte/1")]
    [InlineData("DELETE", "/api/v1/medios-transporte/1")]
    public async Task RutasProtegidasSinTokenRetornan401(string method, string path)
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        var response = await client.SendAsync(request).ConfigureAwait(true);

        // 401 = la ruta existe y la política de auth la protege; 404 = ruta rota.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("POST", "/api/v1/auth/registro")]
    [InlineData("POST", "/api/v1/auth/login")]
    [InlineData("POST", "/api/v1/auth/refresh")]
    [InlineData("POST", "/api/v1/auth/recuperar-password")]
    [InlineData("POST", "/api/v1/auth/restablecer-password")]
    [InlineData("GET", "/api/v1/empresas")]
    [InlineData("GET", "/api/v1/empresas/autocompletar")]
    [InlineData("GET", "/api/v1/empresas/comparar")]
    [InlineData("GET", "/api/v1/empresas/00000000-0000-0000-0000-000000000000")]
    [InlineData("GET", "/api/v1/empresas/00000000-0000-0000-0000-000000000000/rutas")]
    [InlineData("GET", "/api/v1/medios-transporte")]
    public async Task RutasPublicasResuelven(string method, string path)
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method == "POST"
                ? new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                : null,
        };
        var response = await client.SendAsync(request).ConfigureAwait(true);

        // Rutas públicas anónimas: cualquier código distinto de 404 demuestra que
        // la ruta existe (400/422/500 según validación o BD, nunca 404).
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}
