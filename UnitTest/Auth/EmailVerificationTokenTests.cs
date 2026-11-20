using System.Security.Cryptography;
using System.Text;
using ApiSDD.Security;
using FluentAssertions;

namespace UnitTest.Auth;

/// <summary>
/// Pruebas unitarias de tokens criptográficos de verificación y políticas de expiración.
/// </summary>
public sealed class EmailVerificationTokenTests
{
    [Fact]
    public void GenerateSecureTokenProduceCadenaHexConSuficienteEntropia()
    {
        var token1 = CryptoTokenHelper.GenerateSecureToken(32);
        var token2 = CryptoTokenHelper.GenerateSecureToken(32);

        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Length.Should().Be(64); // 32 bytes en hex = 64 chars
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void HashTokenProduceSha256Determinista()
    {
        const string rawToken = "a1b2c3d4e5f60718293a4b5c6d7e8f90";
        var hash1 = CryptoTokenHelper.HashToken(rawToken);
        var hash2 = CryptoTokenHelper.HashToken(rawToken);

        hash1.Should().Be(hash2);
        hash1.Length.Should().Be(64);

        // Verificación manual contra algoritmo de referencia
        var expectedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        var expectedHex = Convert.ToHexString(expectedBytes).ToLowerInvariant();
        hash1.Should().Be(expectedHex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void HashTokenRechazaEntradaInvalida(string? invalidToken)
    {
        Action act = () => CryptoTokenHelper.HashToken(invalidToken!);
        act.Should().Throw<ArgumentException>();
    }
}
