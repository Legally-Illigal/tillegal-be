using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
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
    public async Task Handle_WithValidData_ShouldCreateMessageAndCommit()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var conversationId = Guid.CreateVersion7();
        var conversation = Conversation.Create(userId, "My Chat");
        var created = ConversationMessage.CreateUserMessage(conversationId, "Hello legal AI");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messageService.CreateMessageAsync(conversationId, "user", "Hello legal AI", null, Arg.Any<CancellationToken>())
            .Returns(created);

        var command = new CreateMessageCommand(conversationId, "user", "Hello legal AI");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("user");
        result.Content.Should().Be("Hello legal AI");
        result.ConversationId.Should().Be(conversationId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
