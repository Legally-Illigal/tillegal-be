using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Commands.CreateMessageEvaluation;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class CreateMessageEvaluationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly CreateMessageEvaluationCommandHandler _handler;

    public CreateMessageEvaluationCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new CreateMessageEvaluationCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _aiEvaluationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_AsUser_WithValidAssistantMessage_ShouldCreateEvaluation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");

        var conversation = Conversation.Create(userId, "Test Conversation");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var message = ConversationMessage.CreateAssistantMessage(conversationId, "Trợ lý trả lời...");
        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _aiEvaluationService.ExistsForMessageAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(false);

        var createdEval = AiEvaluationLog.Create(messageId, userId, true, 5, "Tốt", "User");
        _aiEvaluationService.CreateEvaluationAsync(
            messageId, userId, true, 5, "Tốt", "User", Arg.Any<CancellationToken>())
            .Returns(createdEval);

        var command = new CreateMessageEvaluationCommand(conversationId, messageId, true, 5, "Tốt");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Should().Be(messageId);
        result.Rating.Should().Be(5);
        result.IsHelpful.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var command = new CreateMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Admin is not allowed to evaluate*");
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");

        var conversation = Conversation.Create(otherUserId, "Other User Conv");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var command = new CreateMessageEvaluationCommand(conversationId, Guid.NewGuid(), true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*You do not have access to this conversation*");
    }

    [Fact]
    public async Task Handle_WhenMessageIsNotAssistant_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");

        var conversation = Conversation.Create(userId, "My Conv");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var userMessage = ConversationMessage.CreateUserMessage(conversationId, "Tin nhắn của user");
        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(userMessage);

        var command = new CreateMessageEvaluationCommand(conversationId, messageId, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Only AI assistant messages can be evaluated*");
    }

    [Fact]
    public async Task Handle_WhenEvaluationAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");

        var conversation = Conversation.Create(userId, "My Conv");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var message = ConversationMessage.CreateAssistantMessage(conversationId, "Trợ lý trả lời...");
        _messageService.GetMessageByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _aiEvaluationService.ExistsForMessageAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateMessageEvaluationCommand(conversationId, messageId, true, 5, "Tốt");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*An evaluation already exists for this message. Use PUT to update it*");
    }
}

