using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Messages.Commands.DeleteMessage;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Messages;

public sealed class DeleteMessageCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteMessageCommandHandler _handler;

    public DeleteMessageCommandHandlerTests()
    {
        _handler = new DeleteMessageCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new DeleteMessageCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.CreateVersion7());
        _currentUserService.Role.Returns("Admin");

        var command = new DeleteMessageCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Admin is not allowed to delete messages.");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var command = new DeleteMessageCommand(conversationId, Guid.CreateVersion7());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnConversation_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var conversation = Conversation.Create(otherUserId, "Title");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var command = new DeleteMessageCommand(conversationId, Guid.CreateVersion7());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenMessageNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((ConversationMessage?)null);

        var command = new DeleteMessageCommand(conversationId, messageId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenMessageBelongsToDifferentConversation_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var otherConversationId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var message = ConversationMessage.CreateUserMessage(otherConversationId, "Old content");
        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        var command = new DeleteMessageCommand(conversationId, messageId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDeleteMessageAndSave()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var message = ConversationMessage.CreateUserMessage(conversationId, "Content");
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);

        var command = new DeleteMessageCommand(conversationId, message.Id);

        await _handler.Handle(command, CancellationToken.None);

        await _messageService.Received(1).DeleteMessageAsync(message, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
