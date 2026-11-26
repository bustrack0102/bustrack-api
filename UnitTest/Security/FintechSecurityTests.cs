using ApiSDD.Dtos;
using ApiSDD.Security;
using ApiSDD.Validators;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UnitTest.Common;

namespace UnitTest.Security;

/// <summary>Pruebas de seguridad tipo Fintech: anti-inyección, path traversal y unicidad criptográfica.</summary>
public sealed class FintechSecurityTests
{
    private readonly LoginRequestValidator _validator = new();

    private static Argon2idPasswordHasher CreateHasher() =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build());

    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("admin'; DROP TABLE Usuarios;--")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("../../etc/passwd")]
    [InlineData("usuario\0admin")]
    public void LoginValidatorRejectsInjectionTraversalAndNullBytePayloads(string maliciousUserName)
    {
        var result = _validator.Validate(new LoginRequestDto(maliciousUserName, TestDataFactory.Password));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Argon2idProducesDistinctSaltedHashesForIdenticalPasswords()
    {
        var hasher = CreateHasher();
        var hashA = hasher.Hash(TestDataFactory.Password);
        var hashB = hasher.Hash(TestDataFactory.Password);

        hashA.Should().NotBe(hashB);
        hashA.Should().StartWith("$argon2id$v=19$");
        hasher.Verify(hashA, TestDataFactory.Password).Should().BeTrue();
        hasher.Verify(hashB, TestDataFactory.Password).Should().BeTrue();
    }

    [Fact]
    public void Argon2idRejectsTamperedHashAndWhitespacePasswords()
    {
        var hasher = CreateHasher();
        var validHash = hasher.Hash(TestDataFactory.Password);
        var tamperedHash = validHash[..^1] + (validHash[^1] == 'A' ? 'B' : 'A');

        hasher.Verify(tamperedHash, TestDataFactory.Password).Should().BeFalse();
        hasher.Verify(validHash, "   ").Should().BeFalse();
    }
}
