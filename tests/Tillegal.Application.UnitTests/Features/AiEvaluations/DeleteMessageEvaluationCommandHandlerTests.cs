using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Commands.DeleteMessageEvaluation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class DeleteMessageEvaluationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteMessageEvaluationCommandHandler _handler;

    public DeleteMessageEvaluationCommandHandlerTests()
    {
        _handler = new DeleteMessageEvaluationCommandHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _aiEvaluationService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_AsUser_WithOwnEvaluation_ShouldSoftDeleteEvaluation()
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

        var existingEval = AiEvaluationLog.Create(messageId, userId, true, 5, "Tốt", "User");
        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(existingEval);

        var command = new DeleteMessageEvaluationCommand(conversationId, messageId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _aiEvaluationService.Received(1).DeleteEvaluationAsync(existingEval, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var command = new DeleteMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Admin is not allowed to delete evaluations*");
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

        var otherEval = AiEvaluationLog.Create(messageId, otherUserId, true, 5, "Other", "User");
        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(otherEval);

        var command = new DeleteMessageEvaluationCommand(conversationId, messageId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*You can only delete your own evaluation*");
    }
}

