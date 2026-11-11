using ApiSDD.Security;
using FluentAssertions;
using Xunit;

namespace UnitTest.Auth;

public sealed class CryptoTokenHelperTests
{
    [Fact]
    public void GenerateSecureTokenReturnsUniqueHexStrings()
    {
        var token1 = CryptoTokenHelper.GenerateSecureToken();
        var token2 = CryptoTokenHelper.GenerateSecureToken();

        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);
        token1.Length.Should().Be(64); // 32 bytes = 64 hex characters
    }

    [Fact]
    public void HashTokenProducesConsistentSha256()
    {
        var raw = "super-secret-refresh-token";
        var hash1 = CryptoTokenHelper.HashToken(raw);
        var hash2 = CryptoTokenHelper.HashToken(raw);

        hash1.Should().Be(hash2);
        hash1.Length.Should().Be(64);
    }
}
