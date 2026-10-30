using ApiSDD.Validators;
using FluentAssertions;
using UnitTest.Common;

namespace UnitTest.SDDUsuario;

/// <summary>nv-04 Unit: patch boundary rules.</summary>
public sealed class SDDUsuarioUpdateValidatorTests
{
    private readonly SDDUsuarioUpdateValidator _validator = new();

    [Fact]
    public void SingleFieldWithVersionPasses()
    {
        _validator.Validate(TestDataFactory.NewUpdate(email: "other@test.local")).IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyRowVersionFails()
    {
        _validator.Validate(TestDataFactory.NewUpdate(email: "other@test.local", rowVersion: "")).IsValid
            .Should().BeFalse();
    }

    [Fact]
    public void MalformedRowVersionFails()
    {
        _validator.Validate(TestDataFactory.NewUpdate(email: "other@test.local", rowVersion: "!!!")).IsValid
            .Should().BeFalse();
    }

    [Fact]
    public void NoMutableFieldFails()
    {
        _validator.Validate(TestDataFactory.NewUpdate()).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("x")]
    [InlineData("Usuario0123")]
    public void BadUserNameFails(string userName)
    {
        _validator.Validate(TestDataFactory.NewUpdate(userName: userName)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void BadPasswordFails()
    {
        _validator.Validate(TestDataFactory.NewUpdate(password: "weak")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void BadEmailFails()
    {
        _validator.Validate(TestDataFactory.NewUpdate(email: "nope")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void BadRoleFails()
    {
        _validator.Validate(TestDataFactory.NewUpdate(role: "Root")).IsValid.Should().BeFalse();
    }
}
