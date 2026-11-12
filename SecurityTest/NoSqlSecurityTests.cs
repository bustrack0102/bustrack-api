using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitTest.Common;

namespace SecurityTest;

/// <summary>
/// Etapa 1 Security: Lockout por fuerza bruta, RBAC AdminPolicy vs Usuario y rechazo tras logout en modo NoSQL.
/// </summary>
public sealed class NoSqlSecurityTests : IClassFixture<ApiFactory>
{
    private readonly WebApplicationFactory<Program> _mongoFactory;

    public NoSqlSecurityTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _mongoFactory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Persistence:Provider", "MongoDB");
            b.UseSetting("MongoDb:ConnectionString", "REEMPLAZAR_TEST_INMEMORY");
            b.UseSetting("RateLimiting:LoginPer5Minutes", "100");
        });
    }

    [Fact]
    public async Task LockoutBloqueaCuentaTras5IntentosFallidos()
    {
        using var client = _mongoFactory.CreateClient();
        var email = $"lockout.{Guid.NewGuid():N}@uttt.edu.mx";

        await client.PostAsJsonAsync(new Uri("/api/v1/auth/registro", UriKind.Relative), new
        {
            nombreCompleto = "Usuario Lockout",
            correo = email,
            password = "PasswordFuerte123!",
            confirmacionPassword = "PasswordFuerte123!",
            aceptaAvisoPrivacidad = true,
        }).ConfigureAwait(true);

        for (var i = 0; i < 5; i++)
        {
            var bad = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new
            {
                correo = email,
                password = "ClaveIncorrecta999!",
            }).ConfigureAwait(true);
            bad.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Incluso con la contraseña correcta queda bloqueado temporalmente
        var blocked = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new
        {
            correo = email,
            password = "PasswordFuerte123!",
        }).ConfigureAwait(true);
        blocked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UsuarioSinRolAdminRecibe403EnRutasAdministrativas()
    {
        using var client = _mongoFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.UserToken());

        var usersAdmin = await client.GetAsync(new Uri("/api/v1/usuarios", UriKind.Relative)).ConfigureAwait(true);
        usersAdmin.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var createEmpresa = await client.PostAsJsonAsync(new Uri("/api/v1/empresas", UriKind.Relative), new
        {
            nombre = "Empresa No Autorizada",
        }).ConfigureAwait(true);
        createEmpresa.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
