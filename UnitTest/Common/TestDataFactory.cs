using ApiSDD.Dtos;

namespace UnitTest.Common;

/// <summary>Deterministic builders shared by every suite (see 02-test-data).</summary>
public static class TestDataFactory
{
    public const string UserName = "Usuario01";

    public const string Email = "usuario01@test.local";

    public const string Password = "Aa1!bcde";

    public static SDDUsuarioCreateDto NewCreate(
        string userName = UserName,
        string password = Password,
        string email = Email,
        string? role = null) => new(userName, password, email, role);

    public static SDDUsuarioUpdateDto NewUpdate(
        string? userName = null,
        string? password = null,
        string? email = null,
        string? role = null,
        string rowVersion = "AQ==") => new(userName, password, email, role, rowVersion);
}
