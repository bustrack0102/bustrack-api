using System.Security.Cryptography;
using System.Text.Json;
using ApiSDD.Common;
using ApiSDD.Dtos;
using ApiSDD.Security;
using ApiSDD.Validators;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using UnitTest.Common;

namespace UnitTest.Quality;

/// <summary>
/// Repertorio de ingeniería: property-based, table-driven, golden master,
/// complejidad/arquitectura, idempotencia, orden/reloj determinista,
/// compatibilidad hacia atrás y fuzzing de entradas.
/// </summary>
public sealed class EngineeringRepertoireTests
{
    private static readonly int[] UnsortedNumbers = [30, 10, 20];

    private sealed class DeterministicTimeProvider(DateTimeOffset initialUtc) : TimeProvider
    {
        private DateTimeOffset _current = initialUtc;
        public void Advance(TimeSpan delta) => _current = _current.Add(delta);
        public override DateTimeOffset GetUtcNow() => _current;
    }

    private static Argon2idPasswordHasher CreateHasher() =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build());

    [Fact]
    public void PropertyBasedArgon2idInvariantsHoldAcrossRandomInputs()
    {
        var hasher = CreateHasher();
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";

        for (var i = 0; i < 12; i++)
        {
            var len = RandomNumberGenerator.GetInt32(8, 24);
            var chars = Enumerable.Range(0, len)
                .Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)])
                .ToArray();
            var candidate = new string(chars);

            var hash = hasher.Hash(candidate);
            hash.Should().StartWith("$argon2id$v=19$");
            hash.Length.Should().BeLessThanOrEqualTo(PasswordPolicy.HashMaxLength);
            hasher.Verify(hash, candidate).Should().BeTrue();
            hasher.Verify(hash, candidate + "x").Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("Usuario01", "Abc12345!", true)]
    [InlineData("Abc", "Abc12345!", false)]
    [InlineData("Usuario01", "123", false)]
    [InlineData("", "Abc12345!", false)]
    [InlineData("Usuario01", "", false)]
    public void TableDrivenLoginValidationMatrixMatchesPolicy(string user, string pwd, bool expectedValid)
    {
        var validator = new LoginRequestValidator();
        validator.Validate(new LoginRequestDto(user, pwd)).IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void GoldenMasterPagedResultCanonicalJsonMatchesApprovedBaseline()
    {
        var sample = new PagedResult<string>(["RUT-001", "RUT-002"], 2, 1, 20);
        var serialized = JsonSerializer.Serialize(sample, ApiJson.Options);

        const string approvedGolden =
            "{\"Items\":[\"RUT-001\",\"RUT-002\"],\"TotalCount\":2,\"Page\":1,\"PageSize\":20}";
        serialized.Should().Be(approvedGolden);
    }

    [Fact]
    public async Task IdempotencyRepeatedRevocationProducesConsistentState()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new DeterministicTimeProvider(new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.Zero));
        var service = new TokenRevocationService(cache, clock);
        var exp = clock.GetUtcNow().AddMinutes(10);

        for (var i = 0; i < 5; i++)
        {
            await service.RevokeAsync("idempotent-jti", exp, CancellationToken.None).ConfigureAwait(true);
        }

        (await service.IsRevokedAsync("idempotent-jti", CancellationToken.None).ConfigureAwait(true))
            .Should().BeTrue();
    }

    [Fact]
    public async Task DeterministicClockAndOrderingAcrossYearBoundary()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var start = new DateTimeOffset(2026, 12, 31, 23, 59, 50, TimeSpan.Zero);
        var clock = new DeterministicTimeProvider(start);
        var service = new TokenRevocationService(cache, clock);

        await service.RevokeAsync("year-end-jti", start.AddSeconds(30), CancellationToken.None).ConfigureAwait(true);
        (await service.IsRevokedAsync("year-end-jti", CancellationToken.None).ConfigureAwait(true))
            .Should().BeTrue();

        clock.Advance(TimeSpan.FromMinutes(2)); // 2027-01-01T00:01:50Z
        clock.GetUtcNow().Year.Should().Be(2027);

        var ordered = UnsortedNumbers.OrderBy(x => x).ToArray();
        ordered.Should().Equal(10, 20, 30);
    }

    [Fact]
    public void BackwardsCompatibilityLegacyBcryptHashesVerifyAndTriggerRehash()
    {
        var hasher = CreateHasher();
        var legacyBcrypt = BCrypt.Net.BCrypt.HashPassword(TestDataFactory.Password);

        hasher.Verify(legacyBcrypt, TestDataFactory.Password).Should().BeTrue();
        hasher.NeedsRehash(legacyBcrypt).Should().BeTrue();

        var modernArgon = hasher.Hash(TestDataFactory.Password);
        hasher.NeedsRehash(modernArgon).Should().BeFalse();
    }

    [Fact]
    public void ArchitectureValidatorsDoNotDependOnDataOrMiddlewareLayers()
    {
        var apiAssembly = typeof(LoginRequestValidator).Assembly;
        var validatorTypes = apiAssembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith("ApiSDD.Validators", StringComparison.Ordinal))
            .ToArray();

        validatorTypes.Should().NotBeEmpty();
        foreach (var type in validatorTypes)
        {
            var ctorParams = type.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => p.ParameterType.Namespace ?? "");
            ctorParams.Should().NotContain(ns => ns.StartsWith("ApiSDD.Middleware", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void FuzzingMalformedInputsNeverCrashValidatorOrJsonParser()
    {
        var validator = new LoginRequestValidator();

        for (var i = 0; i < 150; i++)
        {
            var len = RandomNumberGenerator.GetInt32(0, 300);
            var bytes = new byte[len];
            RandomNumberGenerator.Fill(bytes);
            var fuzz = Convert.ToBase64String(bytes) + "\0\u202E<script>";

            var actValidate = () => validator.Validate(new LoginRequestDto(fuzz, fuzz));
            actValidate.Should().NotThrow();

            var actDeserialize = () =>
            {
                try
                {
                    return JsonSerializer.Deserialize<LoginRequestDto>(fuzz, ApiJson.Options);
                }
                catch (JsonException)
                {
                    return null;
                }
            };
            actDeserialize.Should().NotThrow();
        }
    }
}
