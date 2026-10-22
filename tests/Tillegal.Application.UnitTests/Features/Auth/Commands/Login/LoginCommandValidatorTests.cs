using FluentAssertions;
using Tillegal.Application.Features.Auth.Commands.Login;

namespace Tillegal.Application.UnitTests.Features.Auth.Commands.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenEmailOrUserNameIsEmpty_ShouldHaveValidationError(string? identifier)
    {
        var command = new LoginCommand(identifier!, "valid_password");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.EmailOrUserName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError(string? password)
    {
        var command = new LoginCommand("testuser", password!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_WhenInputIsValid_ShouldPassValidation()
    {
        var command = new LoginCommand("admin@tillegal.com", "P@ssword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
