using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitTest.Common;

/// <summary>
/// WAF base nv-01: Development + probes ON + Jwt test + cs inválida (ready 503
/// determinista sin contenedor). Derivar con WithWebHostBuilder para Staging,
/// Production y SQL real. Ver 02-test-data.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    public const string InvalidConnectionString =
        "Server=127.0.0.1,1;Database=API_SDD_Test;User Id=sa;Password=Invalid#1;TrustServerCertificate=True;Connection Timeout=2;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:Issuer", "test-issuer");
        builder.UseSetting("Jwt:Audience", "test-audience");
        builder.UseSetting("ConnectionStrings:Default", InvalidConnectionString);
        builder.UseSetting("EnableProviderStates", "true");
        builder.UseSetting(
            "DataProtection:KeysPath",
            Path.Combine(Path.GetTempPath(), "apisdd-tests-dp"));
        builder.UseEnvironment("Development");
    }

    public HttpClient CreateStagingClient() =>
        WithWebHostBuilder(b => b.UseEnvironment("Staging")).CreateClient();

    public HttpClient CreateProductionClient() =>
        WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Production");
            b.UseSetting("EnableProviderStates", "false");
        }).CreateClient();

    public WebApplicationFactory<Program> WithConnectionString(string connectionString) =>
        WithWebHostBuilder(b => b.UseSetting("ConnectionStrings:Default", connectionString));
}
