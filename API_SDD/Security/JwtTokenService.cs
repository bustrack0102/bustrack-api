using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiSDD.Security;

/// <summary>
/// nv-05: HS256 access token emitter. Options are read lazily per call so WAF
/// overrides apply; secrets never leave this boundary.
/// </summary>
internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly IOptionsMonitor<JwtOptions> _options;

    private readonly TimeProvider _time;

    public JwtTokenService(IOptionsMonitor<JwtOptions> options, TimeProvider time)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public (string Token, DateTime Expires) CreateAccessToken(int userId, string role)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);
        var current = _options.CurrentValue;
        if (Encoding.UTF8.GetByteCount(current.Key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key must decode to at least 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(current.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(current.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience is required.");
        }

        var minutes = current.AccessTokenMinutes is >= 1 and <= 60
            ? current.AccessTokenMinutes
            : 15;
        var now = _time.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(minutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(current.Key));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = current.Issuer,
            Audience = current.Audience,
            NotBefore = now,
            IssuedAt = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("role", role),
            ]),
        };

        return (new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor), expires);
    }
}
