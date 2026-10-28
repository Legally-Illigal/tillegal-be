using FluentAssertions;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Application.Features.Conversations.Commands.DeleteConversation;
using Tillegal.Application.Features.Conversations.Commands.UpdateConversation;
using Tillegal.Application.Features.Conversations.Queries.GetConversationById;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class ConversationCommandValidatorTests
{
    [Fact]
    public void CreateConversationCommandValidator_Should_Pass_WhenTitleIsNull()
    {
        var validator = new CreateConversationCommandValidator();
        var result = validator.Validate(new CreateConversationCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateConversationCommandValidator_Should_Pass_WhenTitleIsValid()
    {
        var validator = new CreateConversationCommandValidator();
        var result = validator.Validate(new CreateConversationCommand("Hợp đồng thương mại"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateConversationCommandValidator_Should_Fail_WhenTitleExceedsMaxLength()
    {
        var validator = new CreateConversationCommandValidator();
        var longTitle = new string('a', 201);
        var result = validator.Validate(new CreateConversationCommand(longTitle));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void UpdateConversationCommandValidator_Should_Pass_WhenValid()
    {
        var validator = new UpdateConversationCommandValidator();
        var result = validator.Validate(new UpdateConversationCommand(Guid.NewGuid(), "Tiêu đề mới"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateConversationCommandValidator_Should_Fail_WhenTitleIsEmpty(string? title)
    {
        var validator = new UpdateConversationCommandValidator();
        var result = validator.Validate(new UpdateConversationCommand(Guid.NewGuid(), title!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void UpdateConversationCommandValidator_Should_Fail_WhenIdIsEmpty()
    {
        var validator = new UpdateConversationCommandValidator();
        var result = validator.Validate(new UpdateConversationCommand(Guid.Empty, "Tiêu đề"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void DeleteConversationCommandValidator_Should_Pass_WhenIdIsValid()
    {
        var validator = new DeleteConversationCommandValidator();
        var result = validator.Validate(new DeleteConversationCommand(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteConversationCommandValidator_Should_Fail_WhenIdIsEmpty()
    {
        var validator = new DeleteConversationCommandValidator();
        var result = validator.Validate(new DeleteConversationCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void GetConversationByIdQueryValidator_Should_Pass_WhenIdIsValid()
    {
        var validator = new GetConversationByIdQueryValidator();
        var result = validator.Validate(new GetConversationByIdQuery(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetConversationByIdQueryValidator_Should_Fail_WhenIdIsEmpty()
    {
        var validator = new GetConversationByIdQueryValidator();
        var result = validator.Validate(new GetConversationByIdQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
