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

    /// <summary>
    /// Normaliza la vigencia del access token al rango seguro (1..60 min, por defecto 15),
    /// de modo que emisión y respuestas de autenticación compartan el mismo valor.
    /// </summary>
    public static int ResolveAccessTokenMinutes(int? configuredMinutes) =>
        configuredMinutes is >= 1 and <= 60 ? configuredMinutes.Value : 15;
}
