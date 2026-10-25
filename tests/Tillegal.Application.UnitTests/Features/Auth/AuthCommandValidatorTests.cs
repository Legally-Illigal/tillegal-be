using FluentAssertions;
using Tillegal.Application.Features.Auth.Commands.Register;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;

namespace Tillegal.Application.UnitTests.Features.Auth;

public class AuthCommandValidatorTests
{
    private readonly RegisterCommandValidator _registerValidator = new();
    private readonly VerifyRegistrationCommandValidator _verifyValidator = new();

    [Theory]
    [InlineData("", "Password123", "Full Name")]
    [InlineData("invalid-email", "Password123", "Full Name")]
    [InlineData("valid@example.com", "short", "Full Name")] // < 8 chars
    [InlineData("valid@example.com", "alllowercase1", "Full Name")] // no uppercase
    [InlineData("valid@example.com", "ALLUPPERCASE1", "Full Name")] // no lowercase
    [InlineData("valid@example.com", "NoDigitsPassword", "Full Name")] // no digits
    [InlineData("valid@example.com", "Password123", "")] // empty name
    public void RegisterValidator_WhenInvalid_ShouldHaveValidationErrors(string email, string password, string fullName)
    {
        var command = new RegisterCommand(email, password, fullName);
        var result = _registerValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RegisterValidator_WhenValid_ShouldPass()
    {
        var command = new RegisterCommand("valid@example.com", "SecurePass123", "Nguyen Van A");
        var result = _registerValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "123456")]
    [InlineData("invalid-email", "123456")]
    [InlineData("valid@example.com", "")]
    [InlineData("valid@example.com", "12345")] // 5 digits
    [InlineData("valid@example.com", "1234567")] // 7 digits
    [InlineData("valid@example.com", "12345a")] // non-digits
    public void VerifyValidator_WhenInvalid_ShouldHaveValidationErrors(string email, string otp)
    {
        var command = new VerifyRegistrationCommand(email, otp);
        var result = _verifyValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyValidator_WhenValid_ShouldPass()
    {
        var command = new VerifyRegistrationCommand("valid@example.com", "654321");
        var result = _verifyValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    private readonly Tillegal.Application.Features.Auth.Commands.Login.LoginCommandValidator _loginValidator = new();

    [Theory]
    [InlineData("", "Password123")]
    [InlineData("invalid-email", "Password123")]
    [InlineData("valid@example.com", "")]
    public void LoginValidator_WhenInvalid_ShouldHaveValidationErrors(string email, string password)
    {
        var command = new Tillegal.Application.Features.Auth.Commands.Login.LoginCommand(email, password);
        var result = _loginValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginValidator_WhenValid_ShouldPass()
    {
        var command = new Tillegal.Application.Features.Auth.Commands.Login.LoginCommand("valid@example.com", "ValidPassword123");
        var result = _loginValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
