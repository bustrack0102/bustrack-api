using ApiSDD.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UnitTest.Common;

namespace UnitTest.SDDUsuario;

/// <summary>nv-04 Unit: real Argon2id plus verify-only BCrypt path.</summary>
public sealed class PasswordHasherTests
{
    private static IConfiguration EmptyConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

    private static readonly Argon2idPasswordHasher Hasher = new(EmptyConfig());

    private static readonly string Sample = Hasher.Hash(TestDataFactory.Password);

    private static readonly string Legacy = BCrypt.Net.BCrypt.HashPassword(TestDataFactory.Password);

    [Fact]
    public void HashThenVerifyOk()
    {
        Sample.Should().StartWith("$argon2id$v=19$");
        Sample.Length.Should().BeLessThanOrEqualTo(PasswordPolicy.HashMaxLength);
        Hasher.Verify(Sample, TestDataFactory.Password).Should().BeTrue();
    }

    [Fact]
    public void WrongPasswordFails()
    {
        Hasher.Verify(Sample, "Zz9#wxyz").Should().BeFalse();
    }

    [Fact]
    public void UnknownFormatFails()
    {
        Hasher.Verify("not-a-hash", TestDataFactory.Password).Should().BeFalse();
    }

    [Fact]
    public void NullInputsFail()
    {
        Hasher.Verify(null, TestDataFactory.Password).Should().BeFalse();
        Hasher.Verify(Sample, string.Empty).Should().BeFalse();
        Hasher.NeedsRehash(null).Should().BeTrue();
    }

    [Fact]
    public void CurrentParametersNeedNoRehash()
    {
        Hasher.NeedsRehash(Sample).Should().BeFalse();
    }

    [Fact]
    public void LegacyBcryptVerifiesButNeedsRehash()
    {
        Legacy.Should().MatchRegex("^\\$2[ab]\\$.*");
        Hasher.Verify(Legacy, TestDataFactory.Password).Should().BeTrue();
        Hasher.Verify(Legacy, "Zz9#wxyz").Should().BeFalse();
        Hasher.NeedsRehash(Legacy).Should().BeTrue();
    }

    [Fact]
    public void TamperedHashFails()
    {
        var tampered = Sample[..^1] + (Sample[^1] == 'A' ? 'B' : 'A');

        Hasher.Verify(tampered, TestDataFactory.Password).Should().BeFalse();
    }

    [Fact]
    public void OutOfRangeConfigThrows()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PasswordHasher:MemoryKilobytes"] = "1",
            })
            .Build();

        Action act = () => _ = new Argon2idPasswordHasher(config);

        act.Should().Throw<InvalidOperationException>();
    }
}
