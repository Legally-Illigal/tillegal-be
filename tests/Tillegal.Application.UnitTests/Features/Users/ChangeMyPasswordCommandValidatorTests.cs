using FluentAssertions;
using Tillegal.Application.Features.Users.Commands.ChangeMyPassword;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Users;

public sealed class ChangeMyPasswordCommandValidatorTests
{
    private readonly ChangeMyPasswordCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WhenPasswordIsEmpty_ShouldFail(string? password)
    {
        var command = new ChangeMyPasswordCommand(password!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Theory]
    [InlineData("Short1")]        // < 8 chars
    [InlineData("alllowercase1")]  // no uppercase
    [InlineData("ALLUPPERCASE1")]  // no lowercase
    [InlineData("NoNumberHere")]   // no number
    public void Validate_WhenPasswordDoesNotMeetComplexity_ShouldFail(string weakPassword)
    {
        var command = new ChangeMyPasswordCommand(weakPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Theory]
    [InlineData("ValidPass123")]
    [InlineData("SecureP@ss1")]
    [InlineData("LongStrongPassw0rd!")]
    public void Validate_WhenPasswordIsValid_ShouldPass(string validPassword)
    {
        var command = new ChangeMyPasswordCommand(validPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
