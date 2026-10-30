using System.IdentityModel.Tokens.Jwt;
using ApiSDD.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;
using UnitTest.Common;

namespace UnitTest.Auth;

/// <summary>nv-05 Unit: HS256 issuance maps sub/jti/role with a short expiry.</summary>
public sealed class JwtTokenServiceTests
{
    private sealed class FakeOptions(JwtOptions value) : IOptionsMonitor<JwtOptions>
    {
        public JwtOptions CurrentValue => value;

        public JwtOptions Get(string? name) => value;

        public IDisposable? OnChange(Action<JwtOptions, string?> listener) => NullWatcher.Instance;

        private sealed class NullWatcher : IDisposable
        {
            public static readonly NullWatcher Instance = new();

            public void Dispose()
            {
            }
        }
    }

    private sealed class FakeTime(DateTime utcNow) : TimeProvider
    {
        private readonly DateTime _now = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        public override DateTimeOffset GetUtcNow() => new(_now, TimeSpan.Zero);
    }

    private static JwtTokenService Arrange(DateTime utcNow, string key = ApiFactory.TestJwtKey) =>
        new(new FakeOptions(new JwtOptions
        {
            Key = key,
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenMinutes = 15,
        }), new FakeTime(utcNow));

    [Fact]
    public void EmitsSubJtiRoleWithConfiguredExpiry()
    {
        var now = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);
        var service = Arrange(now);

        var issued = service.CreateAccessToken(7, "Admin");

        issued.Expires.Should().Be(now.AddMinutes(15));
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(issued.Token).Should().BeTrue();
        var token = handler.ReadJwtToken(issued.Token);
        token.Issuer.Should().Be("test-issuer");
        token.Audiences.Should().Contain("test-audience");
        token.Claims.First(static c => c.Type == "sub").Value.Should().Be("7");
        token.Claims.First(static c => c.Type == "role").Value.Should().Be("Admin");
        token.Claims.First(static c => c.Type == "jti").Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ShortKeyFailsFast()
    {
        var service = Arrange(DateTime.UtcNow, "short");

        Action act = () => service.CreateAccessToken(1, "Admin");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OutOfRangeMinutesFallBackToFifteen()
    {
        var now = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);
        var service = new JwtTokenService(
            new FakeOptions(new JwtOptions
            {
                Key = ApiFactory.TestJwtKey,
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenMinutes = 5000,
            }),
            new FakeTime(now));

        var issued = service.CreateAccessToken(1, "Usuario");

        issued.Expires.Should().Be(now.AddMinutes(15));
    }
}
