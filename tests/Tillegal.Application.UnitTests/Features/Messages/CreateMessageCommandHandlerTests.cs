using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Common.Models;
using Tillegal.Application.Features.Messages.Commands.CreateMessage;
using Tillegal.Application.Features.Messages.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Messages;

public sealed class CreateMessageCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IAiServiceClient _aiServiceClient = Substitute.For<IAiServiceClient>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly CreateMessageCommandHandler _handler;

    public CreateMessageCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new MessageMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new CreateMessageCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _aiServiceClient,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new CreateMessageCommand(Guid.CreateVersion7(), "user", "Hello");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.CreateVersion7());
        _currentUserService.Role.Returns("Admin");

        var command = new CreateMessageCommand(Guid.CreateVersion7(), "user", "Hello");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Admin is not allowed to send messages.");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var command = new CreateMessageCommand(conversationId, "user", "Hello");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnConversation_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var conversation = Conversation.Create(otherUserId, "User's Chat");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var command = new CreateMessageCommand(conversationId, "user", "Hello");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WithUserRole_ShouldCallAiWithSingleAgent_AndReturnAssistantMessage()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "My Chat");
        var userMsg = ConversationMessage.CreateUserMessage(conversationId, "Hello legal AI");
        var assistantMsg = ConversationMessage.CreateAssistantMessage(conversationId, "Answer from AI", userMsg.Id);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.CreateMessageAsync(conversationId, "user", "Hello legal AI", null, null, Arg.Any<CancellationToken>())
            .Returns(userMsg);
        _messageService.GetMessagesByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(new List<ConversationMessage>());
        _aiServiceClient.ConsultLegalAsync("Hello legal AI", "single_agent", Arg.Any<IReadOnlyList<ConversationHistoryItem>>(), Arg.Any<CancellationToken>())
            .Returns(new AiConsultationResult("Answer from AI", null));
        _messageService.CreateMessageAsync(conversationId, "assistant", "Answer from AI", userMsg.Id, null, Arg.Any<CancellationToken>())
            .Returns(assistantMsg);

        var command = new CreateMessageCommand(conversationId, "user", "Hello legal AI");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("assistant");
        result.Content.Should().Be("Answer from AI");
        result.ConversationId.Should().Be(conversationId);
        result.ParentMessageId.Should().Be(userMsg.Id);
        await _aiServiceClient.Received(1).ConsultLegalAsync("Hello legal AI", "single_agent", Arg.Any<IReadOnlyList<ConversationHistoryItem>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithLawyerRole_ShouldCallAiWithMultiAgent_AndReturnAssistantMessage()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "Lawyer Case Analysis");
        var userMsg = ConversationMessage.CreateUserMessage(conversationId, "Phân tích điều khoản tranh chấp");
        var assistantMsg = ConversationMessage.CreateAssistantMessage(conversationId, "Tổng hợp ý kiến các chuyên gia pháp lý...", userMsg.Id);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("Lawyer");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.CreateMessageAsync(conversationId, "user", "Phân tích điều khoản tranh chấp", null, null, Arg.Any<CancellationToken>())
            .Returns(userMsg);
        _messageService.GetMessagesByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(new List<ConversationMessage>());
        _aiServiceClient.ConsultLegalAsync("Phân tích điều khoản tranh chấp", "multi_agent", Arg.Any<IReadOnlyList<ConversationHistoryItem>>(), Arg.Any<CancellationToken>())
            .Returns(new AiConsultationResult("Tổng hợp ý kiến các chuyên gia pháp lý...", null));
        _messageService.CreateMessageAsync(conversationId, "assistant", "Tổng hợp ý kiến các chuyên gia pháp lý...", userMsg.Id, null, Arg.Any<CancellationToken>())
            .Returns(assistantMsg);

        var command = new CreateMessageCommand(conversationId, "user", "Phân tích điều khoản tranh chấp");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("assistant");
        result.Content.Should().Be("Tổng hợp ý kiến các chuyên gia pháp lý...");
        await _aiServiceClient.Received(1).ConsultLegalAsync("Phân tích điều khoản tranh chấp", "multi_agent", Arg.Any<IReadOnlyList<ConversationHistoryItem>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAssistantRole_ShouldCreateDirectly_WithoutCallingAi()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "Direct Chat");
        var assistantMsg = ConversationMessage.CreateAssistantMessage(conversationId, "Direct assistant message", null);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.CreateMessageAsync(conversationId, "assistant", "Direct assistant message", null, null, Arg.Any<CancellationToken>())
            .Returns(assistantMsg);

        var command = new CreateMessageCommand(conversationId, "assistant", "Direct assistant message");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("assistant");
        result.Content.Should().Be("Direct assistant message");
        await _aiServiceClient.DidNotReceiveWithAnyArgs().ConsultLegalAsync(default!, default!, default!, default);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidParentMessageId_ShouldCreateMessageWithParentId()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var parentId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "My Chat");
        var parentMessage = ConversationMessage.CreateUserMessage(conversationId, "Parent question");
        var created = ConversationMessage.Create(conversationId, "assistant", "Child branch assistant reply", parentId);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.GetMessageByIdAsync(parentId, Arg.Any<CancellationToken>())
            .Returns(parentMessage);
        _messageService.CreateMessageAsync(conversationId, "assistant", "Child branch assistant reply", parentId, null, Arg.Any<CancellationToken>())
            .Returns(created);

        var command = new CreateMessageCommand(conversationId, "assistant", "Child branch assistant reply", parentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ParentMessageId.Should().Be(parentId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParentMessageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var parentId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "My Chat");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.GetMessageByIdAsync(parentId, Arg.Any<CancellationToken>())
            .Returns((ConversationMessage?)null);

        var command = new CreateMessageCommand(conversationId, "user", "Child question", parentId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenParentMessageBelongsToDifferentConversation_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var otherConversationId = Guid.CreateVersion7();
        var parentId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "My Chat");
        var parentMessage = ConversationMessage.CreateUserMessage(otherConversationId, "Question in other conversation");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.GetMessageByIdAsync(parentId, Arg.Any<CancellationToken>())
            .Returns(parentMessage);

        var command = new CreateMessageCommand(conversationId, "user", "Child question", parentId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
