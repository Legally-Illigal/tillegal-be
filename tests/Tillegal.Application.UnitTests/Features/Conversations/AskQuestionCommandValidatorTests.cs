using FluentAssertions;
using Tillegal.Application.Features.Conversations.Commands.AskQuestion;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class AskQuestionCommandValidatorTests
{
    private readonly AskQuestionCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldPass()
    {
        // Arrange
        var command = new AskQuestionCommand(Guid.NewGuid(), "Luật Đất đai mới có hiệu lực khi nào?");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenConversationIdIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new AskQuestionCommand(Guid.Empty, "Câu hỏi hợp lệ");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AskQuestionCommand.ConversationId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WhenQuestionIsEmptyOrWhitespace_ShouldFail(string? invalidQuestion)
    {
        // Arrange
        var command = new AskQuestionCommand(Guid.NewGuid(), invalidQuestion!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AskQuestionCommand.Question));
    }

    [Fact]
    public void Validate_WhenQuestionExceeds4000Characters_ShouldFail()
    {
        // Arrange
        var command = new AskQuestionCommand(Guid.NewGuid(), new string('x', 4001));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AskQuestionCommand.Question));
    }
}
