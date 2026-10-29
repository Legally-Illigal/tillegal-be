using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Queries.GetConversationById;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class GetConversationByIdQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IMapper _mapper;
    private readonly GetConversationByIdQueryHandler _handler;

    public GetConversationByIdQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
        _handler = new GetConversationByIdQueryHandler(
            _currentUserService,
            _conversationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new GetConversationByIdQuery(Guid.NewGuid()), CancellationToken.None);

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

        var act = () => _handler.Handle(new GetConversationByIdQuery(convId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToDifferentUser_ShouldThrowForbiddenAccessException()
    {
        var currentUserId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(currentUserId);
        var conversation = Conversation.Create(otherUserId, "Hội thoại người khác");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var act = () => _handler.Handle(new GetConversationByIdQuery(convId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToCurrentUser_ShouldReturnDto()
    {
        var userId = Guid.CreateVersion7();
        var convId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        var conversation = Conversation.Create(userId, "Hội thoại của tôi");
        _conversationService.GetConversationByIdAsync(convId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var result = await _handler.Handle(new GetConversationByIdQuery(convId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Hội thoại của tôi");
        result.UserId.Should().Be(userId);
    }
}
