using System.Security.Cryptography;
using System.Text;

namespace ApiSDD.Security;

/// <summary>
/// Helper para hashing SHA-256 de Refresh Tokens y Tokens de Recuperación.
/// </summary>
public static class CryptoTokenHelper
{
    public static string GenerateSecureToken(int byteLength = 32)
    {
        var bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
