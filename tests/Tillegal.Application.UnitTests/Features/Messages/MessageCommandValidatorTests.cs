using FluentAssertions;
using Tillegal.Application.Features.Messages.Commands.CreateMessage;
using Tillegal.Application.Features.Messages.Commands.DeleteMessage;
using Tillegal.Application.Features.Messages.Commands.UpdateMessage;
using Tillegal.Application.Features.Messages.Queries.GetConversationMessages;
using Tillegal.Application.Features.Messages.Queries.GetMessageById;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Messages;

public sealed class MessageCommandValidatorTests
{
    private readonly CreateMessageCommandValidator _createValidator = new();
    private readonly UpdateMessageCommandValidator _updateValidator = new();
    private readonly DeleteMessageCommandValidator _deleteValidator = new();
    private readonly GetConversationMessagesQueryValidator _getMessagesValidator = new();
    private readonly GetMessageByIdQueryValidator _getMessageByIdValidator = new();

    [Fact]
    public void CreateMessage_WhenValid_ShouldNotHaveErrors()
    {
        var command = new CreateMessageCommand(Guid.CreateVersion7(), "user", "Hỏi thủ tục");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateMessage_WhenConversationIdEmpty_ShouldHaveError()
    {
        var command = new CreateMessageCommand(Guid.Empty, "user", "Hỏi thủ tục");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ConversationId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateMessage_WhenRoleEmpty_ShouldHaveError(string? invalidRole)
    {
        var command = new CreateMessageCommand(Guid.CreateVersion7(), invalidRole!, "Hỏi thủ tục");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Role));
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("unknown")]
    public void CreateMessage_WhenRoleNotAllowed_ShouldHaveError(string invalidRole)
    {
        var command = new CreateMessageCommand(Guid.CreateVersion7(), invalidRole, "Hỏi thủ tục");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Role) && e.ErrorMessage.Contains("Role must be"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateMessage_WhenContentEmpty_ShouldHaveError(string? invalidContent)
    {
        var command = new CreateMessageCommand(Guid.CreateVersion7(), "user", invalidContent!);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Content));
    }

    [Fact]
    public void UpdateMessage_WhenValid_ShouldNotHaveErrors()
    {
        var command = new UpdateMessageCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "Nội dung mới");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateMessage_WhenConversationIdEmpty_ShouldHaveError()
    {
        var command = new UpdateMessageCommand(Guid.Empty, Guid.CreateVersion7(), "Nội dung");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ConversationId));
    }

    [Fact]
    public void UpdateMessage_WhenMessageIdEmpty_ShouldHaveError()
    {
        var command = new UpdateMessageCommand(Guid.CreateVersion7(), Guid.Empty, "Nội dung");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MessageId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateMessage_WhenContentEmpty_ShouldHaveError(string? invalidContent)
    {
        var command = new UpdateMessageCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), invalidContent!);
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Content));
    }

    [Fact]
    public void DeleteMessage_WhenValid_ShouldNotHaveErrors()
    {
        var command = new DeleteMessageCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteMessage_WhenConversationIdEmpty_ShouldHaveError()
    {
        var command = new DeleteMessageCommand(Guid.Empty, Guid.CreateVersion7());
        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ConversationId));
    }

    [Fact]
    public void DeleteMessage_WhenMessageIdEmpty_ShouldHaveError()
    {
        var command = new DeleteMessageCommand(Guid.CreateVersion7(), Guid.Empty);
        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MessageId));
    }

    [Fact]
    public void GetConversationMessages_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetConversationMessagesQuery(Guid.CreateVersion7(), null, 5);
        var result = _getMessagesValidator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetConversationMessages_WhenConversationIdEmpty_ShouldHaveError()
    {
        var query = new GetConversationMessagesQuery(Guid.Empty, null, 5);
        var result = _getMessagesValidator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(query.ConversationId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetConversationMessages_WhenLimitZeroOrNegative_ShouldHaveError(int invalidLimit)
    {
        var query = new GetConversationMessagesQuery(Guid.CreateVersion7(), null, invalidLimit);
        var result = _getMessagesValidator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(query.Limit));
    }

    [Fact]
    public void GetConversationMessages_WhenLimitExceedsFifty_ShouldHaveError()
    {
        var query = new GetConversationMessagesQuery(Guid.CreateVersion7(), null, 51);
        var result = _getMessagesValidator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(query.Limit));
    }

    [Fact]
    public void GetMessageById_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetMessageByIdQuery(Guid.CreateVersion7(), Guid.CreateVersion7());
        var result = _getMessageByIdValidator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetMessageById_WhenConversationIdEmpty_ShouldHaveError()
    {
        var query = new GetMessageByIdQuery(Guid.Empty, Guid.CreateVersion7());
        var result = _getMessageByIdValidator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(query.ConversationId));
    }

    [Fact]
    public void GetMessageById_WhenMessageIdEmpty_ShouldHaveError()
    {
        var query = new GetMessageByIdQuery(Guid.CreateVersion7(), Guid.Empty);
        var result = _getMessageByIdValidator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(query.MessageId));
    }
}
