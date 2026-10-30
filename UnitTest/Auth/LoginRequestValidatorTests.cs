using ApiSDD.Dtos;
using ApiSDD.Validators;
using FluentAssertions;
using UnitTest.Common;

namespace UnitTest.Auth;

/// <summary>nv-05 Unit: login boundary rules share constants with nv-04.</summary>
public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void ValidPayloadPasses()
    {
        _validator.Validate(new LoginRequestDto(TestDataFactory.UserName, TestDataFactory.Password))
            .IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Usr1")]
    [InlineData("Usuario0123")]
    [InlineData("Usuario!1")]
    [InlineData("")]
    public void BadUserNameFails(string userName)
    {
        _validator.Validate(new LoginRequestDto(userName, TestDataFactory.Password))
            .IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ab1!c")]
    [InlineData("Abc123!defg")]
    public void BadPasswordShapeFails(string password)
    {
        _validator.Validate(new LoginRequestDto(TestDataFactory.UserName, password))
            .IsValid.Should().BeFalse();
    }

    [Fact]
    public void FullComplexityNotRequiredAtLogin()
    {
        _validator.Validate(new LoginRequestDto(TestDataFactory.UserName, "abcdefgh"))
            .IsValid.Should().BeTrue();
    }
}
