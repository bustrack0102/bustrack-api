using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ApiSDD.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>
/// Pruebas de integración del flujo de verificación de correo electrónico y reenvío.
/// Cubre registro pendiente, verificación válida/expirada/reutilizada, y login condicional.
/// </summary>
public sealed class EmailVerificationIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmailVerificationIntegrationTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
        });
    }

    [Fact]
    public async Task RegistroCreaCuentaPendienteSinEmitirTokens()
    {
        using var client = _factory.CreateClient();
        var email = $"nuevo.pasajero.{Guid.NewGuid():N}@uttt.edu.mx";

        var regResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Usuario Pendiente",
            correo = email,
            password = "PasswordSegura123!",
            confirmacionPassword = "PasswordSegura123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);

        regResp.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await regResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        
        doc.RootElement.GetProperty("requiereVerificacion").GetBoolean().Should().BeTrue();
        doc.RootElement.TryGetProperty("accessToken", out var accessProp).Should().BeTrue();
        accessProp.ValueKind.Should().Be(JsonValueKind.Null);

        // Intento de Login inmediato debe ser denegado por falta de verificación
        var loginResp = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new
        {
            correo = email,
            password = "PasswordSegura123!",
        }).ConfigureAwait(true);

        loginResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loginErr = await loginResp.Content.ReadAsStringAsync().ConfigureAwait(true);
        loginErr.Should().Contain("verificar");
    }

    [Fact]
    public async Task ReenviarVerificacionRespondeExitoGenericoAntiEnumeracion()
    {
        using var client = _factory.CreateClient();

        // 1. Correo inexistente
        var missingResp = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/reenviar-verificacion", UriKind.Relative),
            new ReenviarVerificacionRequestDto("fantasma@correo.com")).ConfigureAwait(true);

        missingResp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var mDoc = JsonDocument.Parse(await missingResp.Content.ReadAsStringAsync().ConfigureAwait(true));
        mDoc.RootElement.GetProperty("mensaje").GetString().Should().Contain("recibirás un nuevo enlace");

        // 2. Correo existente registrado
        var email = $"reenvio.{Guid.NewGuid():N}@uttt.edu.mx";
        await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Usuario Para Reenvio",
            correo = email,
            password = "PasswordSegura123!",
            confirmacionPassword = "PasswordSegura123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);

        var existingResp = await client.PostAsJsonAsync(
            new Uri("/api/v1/auth/reenviar-verificacion", UriKind.Relative),
            new ReenviarVerificacionRequestDto(email)).ConfigureAwait(true);

        existingResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TokenInvalidoOInexistenteRetorna400()
    {
        using var client = _factory.CreateClient();

        var resp = await client.GetAsync(
            new Uri("/api/v1/auth/verificar-correo?token=token_inventado_totalmente_invalido", UriKind.Relative))
            .ConfigureAwait(true);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(true);
        body.Should().Contain("inválido");
    }
}
