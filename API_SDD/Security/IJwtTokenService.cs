namespace ApiSDD.Security;

/// <summary>
/// nv-05: short-lived access token issuance. Implemented by the HS256 service.
/// </summary>
internal interface IJwtTokenService
{
    (string Token, DateTime Expires) CreateAccessToken(int userId, string role);
}
