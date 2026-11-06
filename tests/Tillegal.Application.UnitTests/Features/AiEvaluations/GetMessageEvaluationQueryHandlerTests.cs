using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Application.Features.AiEvaluations.Queries.GetMessageEvaluation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class GetMessageEvaluationQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IMapper _mapper;
    private readonly GetMessageEvaluationQueryHandler _handler;

    public GetMessageEvaluationQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new GetMessageEvaluationQueryHandler(
            _currentUserService,
            _conversationService,
            _messageService,
            _aiEvaluationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_AsUser_WithOwnEvaluation_ShouldReturnDto()
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

        var eval = AiEvaluationLog.Create(messageId, userId, true, 5, "Tuyệt vời", "User");
        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(eval);

        var query = new GetMessageEvaluationQuery(conversationId, messageId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Should().Be(messageId);
        result.EvaluatorUserId.Should().Be(userId);
        result.Rating.Should().Be(5);
        result.IsHelpful.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var query = new GetMessageEvaluationQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Admin is not allowed to access message evaluations*");
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

        var eval = AiEvaluationLog.Create(messageId, otherUserId, true, 5, "Other", "User");
        _aiEvaluationService.GetEvaluationByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(eval);

        var query = new GetMessageEvaluationQuery(conversationId, messageId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*You can only view your own evaluation*");
    }
}

