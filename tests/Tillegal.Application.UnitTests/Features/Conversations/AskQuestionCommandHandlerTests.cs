using System.Text.Json;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Commands.AskQuestion;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class AskQuestionCommandHandlerTests
{
    private readonly IConversationRepository _conversationRepository = Substitute.For<IConversationRepository>();
    private readonly IConversationMessageRepository _messageRepository = Substitute.For<IConversationMessageRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILegalAiService _legalAiService = Substitute.For<ILegalAiService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly ConversationService _conversationService;
    private readonly AskQuestionCommandHandler _handler;

    public AskQuestionCommandHandlerTests()
    {
        _conversationService = new ConversationService(_conversationRepository, _messageRepository);
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);

        _handler = new AskQuestionCommandHandler(
            _currentUserService,
            _conversationService,
            _legalAiService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldStoreBothMessagesAndReturnResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);

        var conversation = new Conversation(
            conversationId,
            userId,
            "Tư vấn doanh nghiệp",
            DateTimeOffset.UtcNow.AddHours(-1),
            null,
            null);

        _conversationRepository.GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        _messageRepository.GetMessagesByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(new List<ConversationMessage>());

        var citations = JsonDocument.Parse("""[{"index": 1, "title": "Luật Doanh nghiệp"}]""");
        _legalAiService.AskAsync(Arg.Any<AiQuestionContext>(), Arg.Any<CancellationToken>())
            .Returns(new AiLegalAnswer("Nội dung tư vấn chi tiết từ AI", citations));

        var command = new AskQuestionCommand(conversationId, "Quy định thời hạn góp vốn?");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserMessage.Should().NotBeNull();
        result.UserMessage.ConversationId.Should().Be(conversationId);
        result.UserMessage.Role.Should().Be("user");
        result.UserMessage.Content.Should().Be("Quy định thời hạn góp vốn?");

        result.AssistantMessage.Should().NotBeNull();
        result.AssistantMessage.ConversationId.Should().Be(conversationId);
        result.AssistantMessage.Role.Should().Be("assistant");
        result.AssistantMessage.Content.Should().Be("Nội dung tư vấn chi tiết từ AI");
        result.AssistantMessage.Citations.Should().NotBeNull();

        await _messageRepository.Received(2).AddAsync(Arg.Any<ConversationMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConversationAlreadyHasMessages_ShouldIncludeHistoryAndAppendNewMessages()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);

        var conversation = new Conversation(
            conversationId,
            userId,
            "Tư vấn doanh nghiệp",
            DateTimeOffset.UtcNow.AddHours(-1),
            null,
            null);

        _conversationRepository.GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var existingMessages = new List<ConversationMessage>
        {
            ConversationMessage.CreateUserMessage(conversationId, "Câu hỏi 1 trước đó"),
            ConversationMessage.CreateAssistantMessage(conversationId, "Câu trả lời 1 trước đó")
        };

        _messageRepository.GetMessagesByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(existingMessages);

        _legalAiService.AskAsync(
                Arg.Is<AiQuestionContext>(ctx =>
                    ctx.ConversationId == conversationId &&
                    ctx.Question == "Câu hỏi tiếp theo lượt 2" &&
                    ctx.History != null &&
                    ctx.History.Count == 2),
                Arg.Any<CancellationToken>())
            .Returns(new AiLegalAnswer("Câu trả lời cho lượt 2", null));

        var command = new AskQuestionCommand(conversationId, "Câu hỏi tiếp theo lượt 2");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserMessage.Content.Should().Be("Câu hỏi tiếp theo lượt 2");
        result.AssistantMessage.Content.Should().Be("Câu trả lời cho lượt 2");

        // Verifies 2 more messages are appended to the conversation
        await _messageRepository.Received(2).AddAsync(Arg.Any<ConversationMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new AskQuestionCommand(Guid.NewGuid(), "Câu hỏi");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");

        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);

        _conversationRepository.GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var command = new AskQuestionCommand(conversationId, "Câu hỏi");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
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

        var command = new AskQuestionCommand(conversationId, "Câu hỏi");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("You do not have permission to access this conversation.");
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
