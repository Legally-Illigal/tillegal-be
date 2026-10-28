using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Conversations.Commands.DeleteConversation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class DeleteConversationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteConversationCommandHandler _handler;

    public DeleteConversationCommandHandlerTests()
    {
        _handler = new DeleteConversationCommandHandler(
            _currentUserService,
            _conversationService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new DeleteConversationCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var act = () => _handler.Handle(new DeleteConversationCommand(convId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToDifferentUser_ShouldThrowForbiddenAccessException()
    {
        var currentUserId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(currentUserId);
        var conversation = Conversation.Create(otherUserId, "Hội thoại");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var act = () => _handler.Handle(new DeleteConversationCommand(convId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCallDeleteAndSaveChangesAsync()
    {
        var userId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        var conversation = Conversation.Create(userId, "Hội thoại");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        await _handler.Handle(new DeleteConversationCommand(convId), CancellationToken.None);

        await _conversationService.Received(1).DeleteConversationAsync(conversation, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
