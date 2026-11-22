using FluentAssertions;
using Tillegal.Application.Features.Users.Commands.UpdateMyProfile;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Users;

public sealed class UpdateMyProfileCommandValidatorTests
{
    private readonly UpdateMyProfileCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WhenFullNameIsEmpty_ShouldFail(string? fullName)
    {
        var command = new UpdateMyProfileCommand(fullName!, "0987654321");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Validate_WhenFullNameExceeds150Chars_ShouldFail()
    {
        var command = new UpdateMyProfileCommand(new string('a', 151), "0987654321");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Theory]
    [InlineData("invalid-phone")]
    [InlineData("12345")]
    [InlineData("abcdefghijk")]
    public void Validate_WhenPhoneNumberIsInvalid_ShouldFail(string invalidPhone)
    {
        var command = new UpdateMyProfileCommand("Valid Name", invalidPhone);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    [Theory]
    [InlineData("0987654321")]
    [InlineData("+84987654321")]
    [InlineData("+84 987-654-321")]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenValid_ShouldPass(string? validPhone)
    {
        var command = new UpdateMyProfileCommand("Valid Name", validPhone);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
