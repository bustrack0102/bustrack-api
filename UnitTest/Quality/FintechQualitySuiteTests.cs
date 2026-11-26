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
/// Suite integral de calidad: concurrencia/race conditions, snapshots JSON,
/// contratos/esquemas, sensibilidad mutacional y mantenibilidad de código.
/// </summary>
public sealed class FintechQualitySuiteTests
{
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static readonly DateTimeOffset BaseUtc = new(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ConcurrencyTokenRevocationUnderParallelLoadHasNoRaceConditions()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TokenRevocationService(cache, new FixedTimeProvider(BaseUtc));
        var expiration = BaseUtc.AddMinutes(30);

        const int workers = 32;
        var jtis = Enumerable.Range(0, workers).Select(i => $"jti-race-{i}").ToArray();

        // Revocación paralela de 32 tokens distintos + lecturas simultáneas
        await Task.WhenAll(
            jtis.Select(jti => service.RevokeAsync(jti, expiration, CancellationToken.None))
        ).ConfigureAwait(true);

        var states = await Task.WhenAll(
            jtis.Select(jti => service.IsRevokedAsync(jti, CancellationToken.None))
        ).ConfigureAwait(true);

        states.Should().AllBeEquivalentTo(true);

        // Mismo JTI revocado simultáneamente por múltiples hilos (idempotente)
        const string sharedJti = "jti-shared-race";
        await Task.WhenAll(
            Enumerable.Range(0, workers)
                .Select(_ => service.RevokeAsync(sharedJti, expiration, CancellationToken.None))
        ).ConfigureAwait(true);

        (await service.IsRevokedAsync(sharedJti, CancellationToken.None).ConfigureAwait(true))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ConcurrencyArgon2idParallelHashAndVerifyIsThreadSafe()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var hasher = new Argon2idPasswordHasher(config);

        var hashes = await Task.WhenAll(
            Enumerable.Range(0, 8).Select(_ => Task.Run(() => hasher.Hash(TestDataFactory.Password)))
        ).ConfigureAwait(true);

        hashes.Should().OnlyHaveUniqueItems();

        var verifications = await Task.WhenAll(
            hashes.Select(h => Task.Run(() => hasher.Verify(h, TestDataFactory.Password)))
        ).ConfigureAwait(true);

        verifications.Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public void SnapshotPagedResultJsonStructureRemainsStable()
    {
        var dto = new LoginRequestDto("UsuarioValido", TestDataFactory.Password);
        var page = new PagedResult<LoginRequestDto>([dto], 1, 1, 20);

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(page, ApiJson.Options));
        var root = doc.RootElement;

        root.EnumerateObject().Select(p => p.Name).Should().Equal(
            "Items",
            "TotalCount",
            "Page",
            "PageSize"
        );
        root.GetProperty("TotalCount").GetInt32().Should().Be(1);
        root.GetProperty("Page").GetInt32().Should().Be(1);
        root.GetProperty("PageSize").GetInt32().Should().Be(20);
        root.GetProperty("Items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public void ContractLoginRequestDtoPreservesExpectedJsonPropertyNames()
    {
        var payload = new LoginRequestDto(TestDataFactory.UserName, TestDataFactory.Password);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payload, ApiJson.Options));
        var props = doc.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);

        props.Should().Contain("strNombreUsuario");
        props.Should().Contain("strPWD");
    }

    [Theory]
    [InlineData("Us", false)] // Por debajo de longitud mínima
    [InlineData("Usuario01", true)] // Válido en frontera
    [InlineData("1234567", false)] // Solo dígitos / inválido
    public void MutationSensitivityLoginValidatorBoundaryConditionsFailOnMutation(
        string candidateUser,
        bool expectedValid)
    {
        var validator = new LoginRequestValidator();
        var result = validator.Validate(new LoginRequestDto(candidateUser, TestDataFactory.Password));
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void MaintainabilityPasswordPolicyConstantsStayWithinSafeBounds()
    {
        PasswordPolicy.HashMaxLength.Should().BeGreaterThanOrEqualTo(64);
        PasswordPolicy.HashMaxLength.Should().BeLessThanOrEqualTo(512);
    }
}
