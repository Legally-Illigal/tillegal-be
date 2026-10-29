using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Messages.Mappings;
using Tillegal.Application.Features.Messages.Queries.GetConversationMessages;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Messages;

public sealed class GetConversationMessagesQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IMapper _mapper;
    private readonly GetConversationMessagesQueryHandler _handler;

    public GetConversationMessagesQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new MessageMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new GetConversationMessagesQueryHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetConversationMessagesQuery(Guid.CreateVersion7());

        var act = () => _handler.Handle(query, CancellationToken.None);

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

        var query = new GetConversationMessagesQuery(conversationId);

        var act = () => _handler.Handle(query, CancellationToken.None);

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

        var query = new GetConversationMessagesQuery(conversationId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnMessages()
    {
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();

        _currentUserService.UserId.Returns(userId);
        var conversation = Conversation.Create(userId, "Title");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var msg1 = ConversationMessage.CreateUserMessage(conversationId, "User question");
        var msg2 = ConversationMessage.CreateAssistantMessage(conversationId, "Assistant reply");
        var messages = new List<ConversationMessage> { msg1, msg2 };

        _messageService.GetMessagesByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(messages);

        var query = new GetConversationMessagesQuery(conversationId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Content.Should().Be("User question");
        result[1].Content.Should().Be("Assistant reply");
    }
}
