namespace ApiSDD.Security;

/// <summary>
/// nv-04: password hashing contract. Argon2id for creation;
/// BCrypt only for legacy verification during migration.
/// </summary>
internal interface IPasswordHasher
{
    string Hash(string plainPassword);

    bool Verify(string? storedHash, string plainPassword);

    bool NeedsRehash(string? storedHash);
}
