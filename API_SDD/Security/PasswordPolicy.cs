namespace ApiSDD.Security;

/// <summary>
/// nv-04: shared credential rules for validators and the login slice (nv-05).
/// </summary>
internal static class PasswordPolicy
{
    public const string UserNamePattern = "^[a-zA-Z0-9]{8,10}$";

    public const int UserNameMinLength = 8;

    public const int UserNameMaxLength = 10;

    public const int PasswordMinLength = 8;

    public const int PasswordMaxLength = 10;

    public const int EmailMaxLength = 50;

    public const int HashMaxLength = 200;

    public static readonly string[] AllowedRoles = ["Admin", "Usuario"];
}
