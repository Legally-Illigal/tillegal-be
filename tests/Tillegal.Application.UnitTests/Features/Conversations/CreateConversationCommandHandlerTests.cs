using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class CreateConversationCommandHandlerTests
{
    private readonly IConversationRepository _conversationRepository = Substitute.For<IConversationRepository>();
    private readonly IConversationMessageRepository _conversationMessageRepository = Substitute.For<IConversationMessageRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly ConversationService _conversationService;
    private readonly CreateConversationCommandHandler _handler;

    public CreateConversationCommandHandlerTests()
    {
        _conversationService = new ConversationService(_conversationRepository, _conversationMessageRepository);
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);

        _handler = new CreateConversationCommandHandler(
            _currentUserService,
            _conversationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldCreateConversationAndReturnResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);

        var command = new CreateConversationCommand("Tư vấn luật dân sự");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.Title.Should().Be("Tư vấn luật dân sự");
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        await _conversationRepository.Received(1)
            .AddAsync(Arg.Is<Conversation>(c => c.UserId == userId && c.Title == "Tư vấn luật dân sự"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new CreateConversationCommand("Title");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");

        await _conversationRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenTitleIsNullOrEmptyOrWhitespace_ShouldDefaultToDefaultTitle(string? title)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        var command = new CreateConversationCommand(title);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Cuộc hội thoại mới");

        await _conversationRepository.Received(1)
            .AddAsync(Arg.Is<Conversation>(c => c.UserId == userId && c.Title == "Cuộc hội thoại mới"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
