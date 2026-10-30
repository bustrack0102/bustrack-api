using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UnitTest.Common;

/// <summary>HS256 tokens for AdminPolicy tests (issuer/audience match the factory).</summary>
public static class JwtTestHelper
{
    public static string CreateToken(
        string role,
        string? algorithm = SecurityAlgorithms.HmacSha256,
        string? key = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key ?? ApiFactory.TestJwtKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(signingKey, algorithm),
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", "1"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("role", role),
            ]),
        };

        return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
    }

    public static string AdminToken() => CreateToken("Admin");

    public static string UserToken() => CreateToken("Usuario");

    public static string ExpiredAdminToken()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.TestJwtKey));
        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims:
            [
                new Claim("sub", "1"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("role", "Admin"),
            ],
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
