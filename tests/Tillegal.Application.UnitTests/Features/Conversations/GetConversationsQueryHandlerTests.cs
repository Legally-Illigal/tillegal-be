using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Queries.GetConversations;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class GetConversationsQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IMapper _mapper;
    private readonly GetConversationsQueryHandler _handler;

    public GetConversationsQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
        _handler = new GetConversationsQueryHandler(
            _currentUserService,
            _conversationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var act = () => _handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserAuthenticated_ShouldReturnConversationsList()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);

        var conversations = new List<Conversation>
        {
            Conversation.Create(userId, "Hội thoại 1"),
            Conversation.Create(userId, "Hội thoại 2")
        };

        _conversationService.GetUserConversationsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(conversations);

        var result = await _handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Hội thoại 1");
        result[1].Title.Should().Be("Hội thoại 2");
    }
}
