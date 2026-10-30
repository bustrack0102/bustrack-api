using ApiSDD.Dtos;
using ApiSDD.Security;
using FluentValidation;

// Validator mirrors its DTO name; Sonar S101 cannot apply.
#pragma warning disable S101

namespace ApiSDD.Validators;

/// <summary>nv-04: creation mirror validator (boundary only, service re-checks uniqueness).</summary>
internal sealed class SDDUsuarioCreateValidator : AbstractValidator<SDDUsuarioCreateDto>
{
    public SDDUsuarioCreateValidator()
    {
        RuleFor(dto => dto.strNombreUsuario)
            .NotEmpty()
            .Length(PasswordPolicy.UserNameMinLength, PasswordPolicy.UserNameMaxLength)
            .Matches(PasswordPolicy.UserNamePattern);

        RuleFor(dto => dto.strPWD)
            .NotEmpty()
            .Length(PasswordPolicy.PasswordMinLength, PasswordPolicy.PasswordMaxLength)
            .Must(HaveLower).WithMessage("One lowercase letter is required.")
            .Must(HaveUpper).WithMessage("One uppercase letter is required.")
            .Must(HaveDigit).WithMessage("One digit is required.")
            .Must(HaveSpecial).WithMessage("One special character is required.");

        RuleFor(dto => dto.strCorreoElectronico)
            .NotEmpty()
            .MaximumLength(PasswordPolicy.EmailMaxLength)
            .EmailAddress();

        RuleFor(dto => dto.strRol)
            .Must(role => role is null || PasswordPolicy.AllowedRoles.Contains(role))
            .WithMessage("Role must be Admin or Usuario.");
    }

    internal static bool HaveLower(string value) => value.Any(char.IsLower);

    internal static bool HaveUpper(string value) => value.Any(char.IsUpper);

    internal static bool HaveDigit(string value) => value.Any(char.IsDigit);

    internal static bool HaveSpecial(string value) => value.Any(static c => !char.IsLetterOrDigit(c));
}
