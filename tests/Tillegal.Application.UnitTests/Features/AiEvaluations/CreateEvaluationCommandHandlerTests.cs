using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Commands.CreateEvaluation;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class CreateEvaluationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly CreateEvaluationCommandHandler _handler;

    public CreateEvaluationCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new CreateEvaluationCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _aiEvaluationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new CreateEvaluationCommand(Guid.CreateVersion7(), true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenMessageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((ConversationMessage?)null);

        var command = new CreateEvaluationCommand(messageId, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenMessageNotAssistantRole_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var message = ConversationMessage.CreateUserMessage(conversationId, "User message");

        _currentUserService.UserId.Returns(userId);
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);

        var command = new CreateEvaluationCommand(message.Id, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Only AI assistant messages can be evaluated.");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var message = ConversationMessage.CreateAssistantMessage(conversationId, "AI response");

        _currentUserService.UserId.Returns(userId);
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var command = new CreateEvaluationCommand(message.Id, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnConversationAndNotAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var message = ConversationMessage.CreateAssistantMessage(conversationId, "AI response");
        var conversation = Conversation.Create(otherUserId, "Hội thoại");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var command = new CreateEvaluationCommand(message.Id, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenEvaluationAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var message = ConversationMessage.CreateAssistantMessage(conversationId, "AI response");
        var conversation = Conversation.Create(userId, "Hội thoại");
        var existingEval = AiEvaluationLog.Create(message.Id, userId, true, 5);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _aiEvaluationService.GetByMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(existingEval);

        var command = new CreateEvaluationCommand(message.Id, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("An evaluation for this message already exists.");
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateEvaluationAndCommit()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var message = ConversationMessage.CreateAssistantMessage(conversationId, "AI response");
        var conversation = Conversation.Create(userId, "Hội thoại");
        var createdEval = AiEvaluationLog.Create(message.Id, userId, true, 5, "Rất tốt");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _aiEvaluationService.GetByMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns((AiEvaluationLog?)null);
        _aiEvaluationService.CreateEvaluationAsync(
            message.Id,
            userId,
            true,
            5,
            "Rất tốt",
            Arg.Any<CancellationToken>())
            .Returns(createdEval);

        var command = new CreateEvaluationCommand(message.Id, true, 5, "Rất tốt");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Should().Be(message.Id);
        result.EvaluatorUserId.Should().Be(userId);
        result.IsHelpful.Should().BeTrue();
        result.Rating.Should().Be(5);
        result.FeedbackComment.Should().Be("Rất tốt");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldAllowEvaluatingOtherUserConversation()
    {
        // Arrange
        var adminUserId = Guid.CreateVersion7();
        var regularUserId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var message = ConversationMessage.CreateAssistantMessage(conversationId, "AI response");
        var conversation = Conversation.Create(regularUserId, "Hội thoại");
        var createdEval = AiEvaluationLog.Create(message.Id, adminUserId, true, 4, "Admin đánh giá");

        _currentUserService.UserId.Returns(adminUserId);
        _currentUserService.Role.Returns("Admin");
        _messageService.GetMessageByIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(message);
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _aiEvaluationService.GetByMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns((AiEvaluationLog?)null);
        _aiEvaluationService.CreateEvaluationAsync(
            message.Id,
            adminUserId,
            true,
            4,
            "Admin đánh giá",
            Arg.Any<CancellationToken>())
            .Returns(createdEval);

        var command = new CreateEvaluationCommand(message.Id, true, 4, "Admin đánh giá");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EvaluatorUserId.Should().Be(adminUserId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

