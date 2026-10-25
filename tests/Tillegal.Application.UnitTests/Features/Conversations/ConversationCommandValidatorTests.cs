using FluentAssertions;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class ConversationCommandValidatorTests
{
    private readonly CreateConversationCommandValidator _validator = new();

    [Theory]
    [InlineData("Valid Title")]
    [InlineData(null)]
    public void Validate_WhenTitleIsValidOrNull_ShouldPass(string? title)
    {
        // Arrange
        var command = new CreateConversationCommand(title);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTitleIsEmptyOrWhitespace_ShouldFail(string? title)
    {
        // Arrange
        var command = new CreateConversationCommand(title);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateConversationCommand.Title));
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldFail()
    {
        // Arrange
        var command = new CreateConversationCommand(new string('a', 256));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateConversationCommand.Title));
    }
}
