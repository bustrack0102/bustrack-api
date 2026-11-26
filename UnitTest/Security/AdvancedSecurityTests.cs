using System.Text.Json;
using ApiSDD.Common;
using ApiSDD.Dtos;
using FluentAssertions;

namespace UnitTest.Security;

/// <summary>Pruebas OWASP API Top 10: Mass Assignment / Over-posting y SSRF.</summary>
public sealed class AdvancedSecurityTests
{
    private static bool IsSafeExternalHttpsUri(string candidate)
    {
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var host = uri.Host.ToLowerInvariant();
        if (host is "localhost" or "127.0.0.1" or "::1" or "169.254.169.254")
        {
            return false;
        }

        return !host.StartsWith("10.", StringComparison.Ordinal)
            && !host.StartsWith("192.168.", StringComparison.Ordinal);
    }

    [Fact]
    public void MassAssignmentExtraPrivilegeFieldsDoNotBindToLoginDto()
    {
        const string overPostedJson =
            "{\"strNombreUsuario\":\"Alumno01\",\"strPWD\":\"Clave123!\",\"rol\":\"ADMIN\",\"isAdmin\":true,\"id\":1}";

        var dto = JsonSerializer.Deserialize<LoginRequestDto>(overPostedJson, ApiJson.Options);
        dto.Should().NotBeNull();
        dto.strNombreUsuario.Should().Be("Alumno01");

        var reserialized = JsonSerializer.Serialize(dto, ApiJson.Options);
        reserialized.Should().NotContain("ADMIN");
        reserialized.Should().NotContain("isAdmin");
    }

    [Theory]
    [InlineData("https://bustrack.uttt.edu.mx/api/v1/ping", true)]
    [InlineData("http://169.254.169.254/latest/meta-data/", false)]
    [InlineData("https://169.254.169.254/latest/meta-data/", false)]
    [InlineData("https://127.0.0.1:8000/admin", false)]
    [InlineData("https://localhost/internal", false)]
    [InlineData("file:///etc/passwd", false)]
    public void SsrfUrlGuardBlocksInternalMetadataAndCleartextTargets(string url, bool expectedSafe)
    {
        IsSafeExternalHttpsUri(url).Should().Be(expectedSafe);
    }
}
