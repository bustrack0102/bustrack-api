using System.Security.Claims;
using System.Text;
using ApiSDD.Data;
using ApiSDD.Security;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace UnitTest.Auth;

/// <summary>Etapa 1: Blacklist JWT distribuida (Redis IDistributedCache) y sanitización de cadena MongoDB.</summary>
public sealed class TokenRevocationDistributedTests
{
    private sealed class FakeTime(DateTimeOffset now) : TimeProvider
    {
        private readonly DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RevokeEscribeEnDistributedCacheConPrefijoYRemainingTtl()
    {
        var distCache = new Mock<IDistributedCache>();
        DistributedCacheEntryOptions? capturedOptions = null;
        byte[]? capturedValue = null;

        distCache
            .Setup(c => c.SetAsync(
                "blacklist:jti-dist-1",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, val, opts, _) =>
            {
                capturedValue = val;
                capturedOptions = opts;
            })
            .Returns(Task.CompletedTask);

        distCache
            .Setup(c => c.GetAsync("blacklist:jti-dist-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedValue);

        var service = new TokenRevocationService(distCache.Object, new FakeTime(Now));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("jti", "jti-dist-1"),
            new Claim("exp", Now.AddMinutes(10).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ]));

        await service.RevokeCurrentAsync(principal, CancellationToken.None).ConfigureAwait(true);

        capturedOptions.Should().NotBeNull();
        capturedOptions.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(10));
        capturedValue.Should().NotBeNull();
        Encoding.UTF8.GetString(capturedValue).Should().NotBeNullOrWhiteSpace();
        (await service.IsRevokedAsync("jti-dist-1", CancellationToken.None).ConfigureAwait(true)).Should().BeTrue();
    }

    [Fact]
    public void SanitizeConnectionStringEliminaCaracteresDeControlEInvisibles()
    {
        const string dirty = "mongodb+srv://user:SecretPass123456\u200B\r\n@cluster0.test.mongodb.net/?appName=Cluster0\u00A0 ";
        var clean = MongoDbContext.SanitizeConnectionString(dirty);

        clean.Should().Be("mongodb+srv://user:SecretPass123456@cluster0.test.mongodb.net/?appName=Cluster0");
    }
}
