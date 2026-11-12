using System.Security.Claims;

namespace ApiSDD.Common;

/// <summary>Helpers compartidos para extraer identidad y rol del JWT validado.</summary>
internal static class ClaimsPrincipalExtensions
{
    public static Guid? TryGetUserId(this ClaimsPrincipal? principal)
    {
        var raw = principal?.FindFirst("sub")?.Value
                  ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) && id != Guid.Empty ? id : null;
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal? principal)
    {
        return principal.TryGetUserId() ?? throw new UnauthorizedException("Token de usuario ausente o inválido.");
    }

    public static bool IsAdmin(this ClaimsPrincipal? principal) =>
        principal is not null
        && (principal.IsInRole("Admin")
            || string.Equals(principal.FindFirst("role")?.Value, "Admin", StringComparison.OrdinalIgnoreCase));
}
