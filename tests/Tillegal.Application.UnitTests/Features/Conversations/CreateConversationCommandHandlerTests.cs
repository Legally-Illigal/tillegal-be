using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class CreateConversationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly CreateConversationCommandHandler _handler;

    public CreateConversationCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
        _handler = new CreateConversationCommandHandler(
            _currentUserService,
            _conversationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var act = () => _handler.Handle(new CreateConversationCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenTitleIsNull_ShouldUseDefaultTitleAndSave()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);

        var created = Conversation.Create(userId, "Cuộc hội thoại mới");
        _conversationService.CreateConversationAsync(userId, "Cuộc hội thoại mới", Arg.Any<CancellationToken>())
            .Returns(created);

        // Act
        var result = await _handler.Handle(new CreateConversationCommand(null), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Cuộc hội thoại mới");
        result.UserId.Should().Be(userId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTitleProvided_ShouldUseTrimmedTitleAndSave()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);

        var created = Conversation.Create(userId, "Tư vấn thuế");
        _conversationService.CreateConversationAsync(userId, "Tư vấn thuế", Arg.Any<CancellationToken>())
            .Returns(created);

        // Act
        var result = await _handler.Handle(new CreateConversationCommand("  Tư vấn thuế  "), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Tư vấn thuế");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
