using ApiSDD.Common;
using ApiSDD.Dtos;
using ApiSDD.Security;
using FluentValidation;

// Validator mirrors its DTO name; Sonar S101 cannot apply.
#pragma warning disable S101

namespace ApiSDD.Validators;

/// <summary>nv-04: patch mirror validator. Requires at least one mutable field.</summary>
internal sealed class SDDUsuarioUpdateValidator : AbstractValidator<SDDUsuarioUpdateDto>
{
    public SDDUsuarioUpdateValidator()
    {
        RuleFor(dto => dto.RowVersion)
            .NotEmpty()
            .Must(static wire => RowVersionCodec.TryDecode(wire, out _))
            .WithMessage("RowVersion must be valid base64.");

        RuleFor(dto => dto)
            .Must(static dto => dto.strNombreUsuario is not null
                || dto.strPWD is not null
                || dto.strCorreoElectronico is not null
                || dto.strRol is not null)
            .WithMessage("At least one mutable field is required.");

        When(dto => dto.strNombreUsuario is not null, () =>
        {
            RuleFor(dto => dto.strNombreUsuario!)
                .Length(PasswordPolicy.UserNameMinLength, PasswordPolicy.UserNameMaxLength)
                .Matches(PasswordPolicy.UserNamePattern);
        });

        When(dto => dto.strPWD is not null, () =>
        {
            RuleFor(dto => dto.strPWD!)
                .Length(PasswordPolicy.PasswordMinLength, PasswordPolicy.PasswordMaxLength)
                .Must(SDDUsuarioCreateValidator.HaveLower).WithMessage("One lowercase letter is required.")
                .Must(SDDUsuarioCreateValidator.HaveUpper).WithMessage("One uppercase letter is required.")
                .Must(SDDUsuarioCreateValidator.HaveDigit).WithMessage("One digit is required.")
                .Must(SDDUsuarioCreateValidator.HaveSpecial).WithMessage("One special character is required.");
        });

        When(dto => dto.strCorreoElectronico is not null, () =>
        {
            RuleFor(dto => dto.strCorreoElectronico!)
                .MaximumLength(PasswordPolicy.EmailMaxLength)
                .EmailAddress();
        });

        When(dto => dto.strRol is not null, () =>
        {
            RuleFor(dto => dto.strRol!)
                .Must(static role => PasswordPolicy.AllowedRoles.Contains(role))
                .WithMessage("Role must be Admin or Usuario.");
        });
    }
}
