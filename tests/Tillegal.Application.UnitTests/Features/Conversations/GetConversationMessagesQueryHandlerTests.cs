using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Queries.GetConversationMessages;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class GetConversationMessagesQueryHandlerTests
{
    private readonly IConversationRepository _conversationRepository = Substitute.For<IConversationRepository>();
    private readonly IConversationMessageRepository _messageRepository = Substitute.For<IConversationMessageRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMapper _mapper;
    private readonly ConversationService _conversationService;
    private readonly GetConversationMessagesQueryHandler _handler;

    public GetConversationMessagesQueryHandlerTests()
    {
        _conversationService = new ConversationService(_conversationRepository, _messageRepository);
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);

        _handler = new GetConversationMessagesQueryHandler(
            _currentUserService,
            _conversationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnMappedMessages()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);

        var conversation = new Conversation(
            conversationId,
            userId,
            "Tiêu đề",
            DateTimeOffset.UtcNow,
            null,
            null);

        _conversationRepository.GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var messages = new List<ConversationMessage>
        {
            ConversationMessage.CreateUserMessage(conversationId, "Câu hỏi 1"),
            ConversationMessage.CreateAssistantMessage(conversationId, "Câu trả lời 1")
        };

        _messageRepository.GetMessagesByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(messages);

        var query = new GetConversationMessagesQuery(conversationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Role.Should().Be("user");
        result[0].Content.Should().Be("Câu hỏi 1");
        result[1].Role.Should().Be("assistant");
        result[1].Content.Should().Be("Câu trả lời 1");
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetConversationMessagesQuery(Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(Guid.NewGuid());

        _conversationRepository.GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var query = new GetConversationMessagesQuery(conversationId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToDifferentUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserService.UserId.Returns(currentUserId);

        var conversation = new Conversation(
            conversationId,
            ownerUserId,
            "Tiêu đề",
            DateTimeOffset.UtcNow,
            null,
            null);

        _conversationRepository.GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var query = new GetConversationMessagesQuery(conversationId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
