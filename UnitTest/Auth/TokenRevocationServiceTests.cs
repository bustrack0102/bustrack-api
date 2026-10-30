using System.Security.Claims;
using ApiSDD.Common;
using ApiSDD.Security;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace UnitTest.Auth;

/// <summary>nv-06 Unit: blacklist with remaining-life TTL, fail-closed on bad claims.</summary>
public sealed class TokenRevocationServiceTests
{
    private sealed class FakeTime(DateTimeOffset now) : TimeProvider
    {
        private readonly DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;
    }

    private static readonly DateTimeOffset Now = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    private static TokenRevocationService Arrange(IMemoryCache cache) =>
        new(cache, new FakeTime(Now));

    private static ClaimsPrincipal Principal(string? jti, string? exp)
    {
        var claims = new List<Claim>();
        if (jti is not null)
        {
            claims.Add(new Claim("jti", jti));
        }

        if (exp is not null)
        {
            claims.Add(new Claim("exp", exp));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    [Fact]
    public async Task RevokeThenIsRevoked()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = Arrange(cache);

        (await service.IsRevokedAsync("abc", CancellationToken.None).ConfigureAwait(true)).Should().BeFalse();
        await service.RevokeAsync("abc", Now.AddMinutes(15), CancellationToken.None).ConfigureAwait(true);
        (await service.IsRevokedAsync("abc", CancellationToken.None).ConfigureAwait(true)).Should().BeTrue();
    }

    [Fact]
    public async Task ExpiredTokenIsNoop()
    {
        var cache = new Mock<IMemoryCache>();
        var service = Arrange(cache.Object);

        await service.RevokeAsync("abc", Now.AddMinutes(-1), CancellationToken.None).ConfigureAwait(true);

        cache.Verify(static c => c.CreateEntry(It.IsAny<object>()), Times.Never);
        (await service.IsRevokedAsync("abc", CancellationToken.None).ConfigureAwait(true)).Should().BeFalse();
    }

    [Fact]
    public async Task TtlEqualsRemainingLife()
    {
        var cache = new Mock<IMemoryCache>();
        var entry = new Mock<ICacheEntry>();
        entry.SetupProperty(static e => e.AbsoluteExpirationRelativeToNow);
        cache.Setup(static c => c.CreateEntry(It.IsAny<object>())).Returns(entry.Object);
        var service = Arrange(cache.Object);

        await service.RevokeAsync("abc", Now.AddMinutes(15), CancellationToken.None).ConfigureAwait(true);

        entry.Object.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task MissingJtiDenies()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = Arrange(cache);

        var act = () => service.RevokeCurrentAsync(
            Principal(null, Now.AddMinutes(15).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().ConfigureAwait(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-number")]
    [InlineData("99999999999999999999")]
    public async Task BadExpDenies(string? exp)
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = Arrange(cache);

        var act = () => service.RevokeCurrentAsync(Principal("abc", exp), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().ConfigureAwait(true);
    }

    [Fact]
    public async Task CurrentUserRevokes()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = Arrange(cache);
        var user = Principal("abc", Now.AddMinutes(15).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));

        await service.RevokeCurrentAsync(user, CancellationToken.None).ConfigureAwait(true);

        (await service.IsRevokedAsync("abc", CancellationToken.None).ConfigureAwait(true)).Should().BeTrue();
    }
}
