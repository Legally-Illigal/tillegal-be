using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Commands.UpdateConversation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class UpdateConversationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly UpdateConversationCommandHandler _handler;

    public UpdateConversationCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
        _handler = new UpdateConversationCommandHandler(
            _currentUserService,
            _conversationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new UpdateConversationCommand(Guid.NewGuid(), "Tiêu đề mới"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.CreateVersion7());
        _currentUserService.Role.Returns("Admin");

        var act = () => _handler.Handle(new UpdateConversationCommand(Guid.NewGuid(), "Tiêu đề mới"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Admin is not allowed to update conversations.");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var act = () => _handler.Handle(new UpdateConversationCommand(convId, "Tiêu đề mới"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnConversation_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();
        var conversation = Conversation.Create(otherUserId, "Tiêu đề cũ");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var act = () => _handler.Handle(new UpdateConversationCommand(convId, "Tiêu đề mới"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateTitleAndSave()
    {
        var userId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "Tiêu đề cũ");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var updated = Conversation.Create(userId, "Tiêu đề mới");
        _conversationService.UpdateConversationTitleAsync(conversation, "Tiêu đề mới", Arg.Any<CancellationToken>())
            .Returns(updated);

        var result = await _handler.Handle(new UpdateConversationCommand(convId, "Tiêu đề mới"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Tiêu đề mới");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
