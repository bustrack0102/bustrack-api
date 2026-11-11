namespace ApiSDD.Security;

/// <summary>
/// nv-04: shared credential rules for validators and the login slice (nv-05).
/// EP-AU-01/08: reglas de contraseña y correo para el flujo V2.
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

    // EP-AU-01: mínimo 8 caracteres con letras y números.
    private static readonly System.Text.RegularExpressions.Regex HasLetter =
        new("[A-Za-z]", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromMilliseconds(200));

    private static readonly System.Text.RegularExpressions.Regex HasDigit =
        new("[0-9]", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromMilliseconds(200));

    // Formato de correo pragmático: local@dominio.tld sin espacios.
    private static readonly System.Text.RegularExpressions.Regex EmailShape =
        new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromMilliseconds(200));

    /// <summary>Valida la fuerza de la contraseña (≥8 caracteres, letras y números).</summary>
    public static void EnsureStrongPassword(string password)
    {
        if (password.Length < 8)
        {
            throw new BadHttpRequestException("La contraseña debe contener al menos 8 caracteres.");
        }

        if (!HasLetter.IsMatch(password) || !HasDigit.IsMatch(password))
        {
            throw new BadHttpRequestException("La contraseña debe incluir letras y números.");
        }
    }

    /// <summary>Valida el formato básico de un correo electrónico.</summary>
    public static bool IsValidEmail(string correo) =>
        EmailShape.IsMatch(correo) && correo.Length <= 150;
}
