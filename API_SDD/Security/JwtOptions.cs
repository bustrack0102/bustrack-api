namespace ApiSDD.Security;

/// <summary>
/// nv-05: JWT issuance settings bound from the Jwt section. AccessTokenMinutes is optional.
/// </summary>
internal sealed class JwtOptions
{
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
}
