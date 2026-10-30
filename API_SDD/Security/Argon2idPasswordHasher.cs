using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace ApiSDD.Security;

/// <summary>
/// nv-04: real Argon2id (64MB/3iter per constitution) with PHC encoding.
/// BCrypt hashes ($2a$/$2b$) verify only, never created.
/// </summary>
internal sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int SaltLength = 16;

    private const int HashLength = 32;

    private const int ArgonVersion = 19;

    private readonly int _memoryKilobytes;

    private readonly int _iterations;

    private readonly int _parallelism;

    public Argon2idPasswordHasher(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _memoryKilobytes = configuration.GetValue<int?>("PasswordHasher:MemoryKilobytes") ?? 65536;
        _iterations = configuration.GetValue<int?>("PasswordHasher:Iterations") ?? 3;
        _parallelism = configuration.GetValue<int?>("PasswordHasher:DegreeOfParallelism") ?? 4;

        if (_memoryKilobytes is < 8192 or > 1048576)
        {
            throw new InvalidOperationException("PasswordHasher:MemoryKilobytes out of range (8192..1048576).");
        }

        if (_iterations is < 1 or > 10)
        {
            throw new InvalidOperationException("PasswordHasher:Iterations out of range (1..10).");
        }

        if (_parallelism is < 1 or > 16)
        {
            throw new InvalidOperationException("PasswordHasher:DegreeOfParallelism out of range (1..16).");
        }
    }

    public string Hash(string plainPassword)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainPassword);
        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Compute(plainPassword, salt, _memoryKilobytes, _iterations, _parallelism);
        var encoded = $"$argon2id$v={ArgonVersion}$m={_memoryKilobytes},t={_iterations},p={_parallelism}"
            + $"${Trim(Convert.ToBase64String(salt))}${Trim(Convert.ToBase64String(hash))}";

        if (encoded.Length > PasswordPolicy.HashMaxLength)
        {
            throw new InvalidOperationException("PHC output exceeds strPWD capacity.");
        }

        return encoded;
    }

    public bool Verify(string? storedHash, string plainPassword)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(plainPassword))
        {
            return false;
        }

        if (storedHash.StartsWith("$argon2id$", StringComparison.Ordinal))
        {
            return VerifyArgon2id(storedHash, plainPassword);
        }

        if (storedHash.StartsWith("$2a$", StringComparison.Ordinal)
            || storedHash.StartsWith("$2b$", StringComparison.Ordinal))
        {
            return VerifyBcrypt(storedHash, plainPassword);
        }

        return false;
    }

    public bool NeedsRehash(string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
        {
            return true;
        }

        if (!storedHash.StartsWith("$argon2id$", StringComparison.Ordinal))
        {
            return true;
        }

        return !MatchesParameters(storedHash);
    }

    private static byte[] Compute(string password, byte[] salt, int memory, int iterations, int parallelism)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = parallelism;
        argon2.Iterations = iterations;
        argon2.MemorySize = memory;
        return argon2.GetBytes(HashLength);
    }

    private static bool VerifyArgon2id(string encoded, string plainPassword)
    {
        if (!TryParse(encoded, out var memory, out var iterations, out var parallelism, out var salt, out var expected))
        {
            return false;
        }

        var actual = Compute(plainPassword, salt, memory, iterations, parallelism);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private bool MatchesParameters(string encoded) =>
        TryParse(encoded, out var memory, out var iterations, out var parallelism, out _, out _)
        && memory == _memoryKilobytes
        && iterations == _iterations
        && parallelism == _parallelism;

    private static bool VerifyBcrypt(string storedHash, string plainPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }

    private static bool TryParse(
        string encoded,
        out int memory,
        out int iterations,
        out int parallelism,
        out byte[] salt,
        out byte[] hash)
    {
        memory = 0;
        iterations = 0;
        parallelism = 0;
        salt = [];
        hash = [];
        var parts = encoded.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5 || parts[0] != "argon2id" || parts[1] != "v=19")
        {
            return false;
        }

        foreach (var pair in parts[2].Split(','))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length != 2 || !int.TryParse(kv[1], out var value))
            {
                return false;
            }

            if (kv[0] == "m")
            {
                memory = value;
            }
            else if (kv[0] == "t")
            {
                iterations = value;
            }
            else if (kv[0] == "p")
            {
                parallelism = value;
            }
            else
            {
                return false;
            }
        }

        try
        {
            salt = Convert.FromBase64String(Pad(parts[3]));
            hash = Convert.FromBase64String(Pad(parts[4]));
        }
        catch (FormatException)
        {
            return false;
        }

        return memory > 0 && iterations > 0 && parallelism > 0
            && salt.Length == SaltLength && hash.Length == HashLength;
    }

    private static string Trim(string base64) => base64.TrimEnd('=');

    private static string Pad(string trimmed) => trimmed.PadRight(trimmed.Length + ((4 - (trimmed.Length % 4)) % 4), '=');
}
