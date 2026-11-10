using FluentValidation.TestHelper;
using Tillegal.Application.Features.Admin.Users.Commands.GrantLawyerRole;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Admin;

public sealed class GrantLawyerRoleCommandValidatorTests
{
    private readonly GrantLawyerRoleCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new GrantLawyerRoleCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveValidationError()
    {
        var command = new GrantLawyerRoleCommand(Guid.CreateVersion7());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

