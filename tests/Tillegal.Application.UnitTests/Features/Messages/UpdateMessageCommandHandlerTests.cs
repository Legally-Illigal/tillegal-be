using System.Text.Json;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Messages.Commands.UpdateMessage;
using Tillegal.Application.Features.Messages.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Messages;

public sealed class UpdateMessageCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly UpdateMessageCommandHandler _handler;

    public UpdateMessageCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new MessageMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new UpdateMessageCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new UpdateMessageCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "New content");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var command = new UpdateMessageCommand(conversationId, Guid.CreateVersion7(), "New content");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToDifferentUser_ShouldThrowForbiddenAccessException()
    {
        var currentUserId = Guid.CreateVersion7();
        var conversationOwnerId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(currentUserId);
        var conversation = Conversation.Create(conversationOwnerId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var command = new UpdateMessageCommand(conversationId, Guid.CreateVersion7(), "New content");

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
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((ConversationMessage?)null);

        var command = new UpdateMessageCommand(conversationId, messageId, "New content");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenMessageConversationIdMismatched_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var otherConversationId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var message = ConversationMessage.CreateUserMessage(otherConversationId, "Old content");
        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        var command = new UpdateMessageCommand(conversationId, messageId, "New content");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateMessageAndSave()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var message = ConversationMessage.CreateUserMessage(conversationId, "Old content");
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);

        var citations = JsonDocument.Parse("""{"updated": true}""");
        var command = new UpdateMessageCommand(conversationId, message.Id, "Updated content", citations);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _messageService.Received(1).UpdateMessageAsync(message, "Updated content", citations, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
