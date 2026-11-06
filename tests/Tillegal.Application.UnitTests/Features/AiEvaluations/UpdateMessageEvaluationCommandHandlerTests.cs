using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Commands.UpdateMessageEvaluation;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class UpdateMessageEvaluationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly UpdateMessageEvaluationCommandHandler _handler;

    public UpdateMessageEvaluationCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new UpdateMessageEvaluationCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _aiEvaluationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_AsUser_WithOwnEvaluation_ShouldUpdateEvaluation()
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

        var existingEval = AiEvaluationLog.Create(messageId, userId, true, 4, "Khá ổn", "User");
        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(existingEval);

        var command = new UpdateMessageEvaluationCommand(conversationId, messageId, true, 5, "Cập nhật thành 5 sao");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _aiEvaluationService.Received(1).UpdateEvaluationAsync(
            existingEval, true, 5, "Cập nhật thành 5 sao", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var command = new UpdateMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), true, 5, "Update");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Admin is not allowed to evaluate*");
    }

    [Fact]
    public async Task Handle_WhenEvaluationNotFound_ShouldThrowNotFoundException()
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

        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((AiEvaluationLog?)null);

        var command = new UpdateMessageEvaluationCommand(conversationId, messageId, true, 5, "Update");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenEvaluationBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
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

        var otherUserEval = AiEvaluationLog.Create(messageId, otherUserId, true, 4, "Other user's eval", "User");
        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(otherUserEval);

        var command = new UpdateMessageEvaluationCommand(conversationId, messageId, true, 5, "Update");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*You can only modify your own evaluation*");
    }
}

