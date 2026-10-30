using ApiSDD.Dtos;
using ApiSDD.Security;
using FluentValidation;

// Validator mirrors its DTO name; Sonar S101 cannot apply.
#pragma warning disable S101

namespace ApiSDD.Validators;

/// <summary>nv-05: login boundary rules. Shares user-name constants with nv-04 to avoid drift.</summary>
internal sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(dto => dto.strNombreUsuario)
            .NotEmpty()
            .Length(PasswordPolicy.UserNameMinLength, PasswordPolicy.UserNameMaxLength)
            .Matches(PasswordPolicy.UserNamePattern);

        RuleFor(dto => dto.strPWD)
            .NotEmpty()
            .Length(PasswordPolicy.PasswordMinLength, PasswordPolicy.PasswordMaxLength);
    }
}
