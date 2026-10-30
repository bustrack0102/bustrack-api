using System.Security.Claims;

namespace ApiSDD.Security;

/// <summary>
/// nv-06: access-token revocation backed by IMemoryCache. TTL is the remaining lifetime.
/// </summary>
public interface ITokenRevocationService
{
    const string KeyPrefix = "blacklist:";

    Task RevokeAsync(string jti, DateTimeOffset exp, CancellationToken cancellationToken);

    Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken);

    Task RevokeCurrentAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}
