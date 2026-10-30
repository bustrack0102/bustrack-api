using ApiSDD.Validators;
using FluentAssertions;
using UnitTest.Common;

namespace UnitTest.SDDUsuario;

/// <summary>nv-04 Unit: creation boundary rules.</summary>
public sealed class SDDUsuarioCreateValidatorTests
{
    private readonly SDDUsuarioCreateValidator _validator = new();

    [Fact]
    public void ValidPayloadPasses()
    {
        _validator.Validate(TestDataFactory.NewCreate()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void AdminRolePassesAtBoundary()
    {
        _validator.Validate(TestDataFactory.NewCreate(role: "Admin")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Usr1")]
    [InlineData("Usuario0123")]
    [InlineData("Usuario!1")]
    [InlineData("")]
    public void BadUserNameFails(string userName)
    {
        _validator.Validate(TestDataFactory.NewCreate(userName: userName)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("Abc123de")]
    [InlineData("abc123!d")]
    [InlineData("ABC123!D")]
    [InlineData("Abcdef!g")]
    [InlineData("Ab1!c")]
    [InlineData("Abc123!defg")]
    public void BadPasswordFails(string password)
    {
        _validator.Validate(TestDataFactory.NewCreate(password: password)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void BadEmailFails(string email)
    {
        _validator.Validate(TestDataFactory.NewCreate(email: email)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UnknownRoleFails()
    {
        _validator.Validate(TestDataFactory.NewCreate(role: "Root")).IsValid.Should().BeFalse();
    }
}
