using ApiSDD.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace UnitTest.Auth;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData("Ab1cdefg")]
    [InlineData("1234567a")]
    [InlineData("Segura2026")]
    public void StrongPasswordDoesNotThrow(string password)
    {
        var act = () => PasswordPolicy.EnsureStrongPassword(password);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Ab1cd")] // demasiado corta
    [InlineData("abcdefg")] // sin números
    [InlineData("1234567")] // sin letras
    public void WeakPasswordThrows400(string password)
    {
        var act = () => PasswordPolicy.EnsureStrongPassword(password);
        act.Should().Throw<BadHttpRequestException>();
    }

    [Theory]
    [InlineData("usuario@dominio.com")]
    [InlineData("a.b@sub.dominio.org")]
    [InlineData("Nombre+tag@empresa.mx")]
    public void ValidEmailsAreAccepted(string correo)
    {
        PasswordPolicy.IsValidEmail(correo).Should().BeTrue();
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("dos@arrobas@invalido.com")]
    [InlineData("con espacio@dominio.com")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("")]
    public void InvalidEmailsAreRejected(string correo)
    {
        PasswordPolicy.IsValidEmail(correo).Should().BeFalse();
    }

    [Fact]
    public void EmailLongerThan150IsRejected()
    {
        var largo = new string('a', 148) + "@d.com";
        PasswordPolicy.IsValidEmail(largo).Should().BeFalse();
    }
}
