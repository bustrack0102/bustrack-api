using System.Security.Cryptography;

namespace ApiSDD.Middleware;

/// <summary>
/// Headers de seguridad outermost + CSP con nonce único por request.
/// Exención scalar/openapi (sin CSP) según constitución §6.
/// </summary>
internal sealed class SecurityHeadersMiddleware
{
    public const string NonceItemKey = "CspNonce";

    private static readonly string[] ExemptPrefixes = ["/scalar", "/openapi"];

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public static string GenerateNonce() => CreateNonce(RandomNumberGenerator.GetBytes(16));

    public static string CreateNonce(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static bool IsExempt(PathString path) =>
        ExemptPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var nonce = GenerateNonce();
        context.Items[NonceItemKey] = nonce;
        ApplyHeaders(context, nonce);
        await _next(context).ConfigureAwait(false);
    }

    private static void ApplyHeaders(HttpContext context, string nonce)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["X-XSS-Protection"] = "0";
        if (!IsExempt(context.Request.Path))
        {
            headers.ContentSecurityPolicy = string.Concat("default-src 'self'; script-src 'nonce-", nonce, "'");
        }
    }
}
