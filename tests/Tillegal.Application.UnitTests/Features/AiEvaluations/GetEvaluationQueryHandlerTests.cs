using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluationSummary;
using Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationById;
using Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationByMessageId;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class GetEvaluationQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IConversationMessageService _messageService = Substitute.For<IConversationMessageService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IMapper _mapper;

    public GetEvaluationQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var evalId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _aiEvaluationService.GetByIdAsync(evalId, Arg.Any<CancellationToken>())
            .Returns((AiEvaluationLog?)null);

        var handler = new GetEvaluationByIdQueryHandler(_currentUserService, _aiEvaluationService, _mapper);
        var act = () => handler.Handle(new GetEvaluationByIdQuery(evalId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_WhenNotOwnerAndNotAdmin_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), otherUserId, true, 5);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var handler = new GetEvaluationByIdQueryHandler(_currentUserService, _aiEvaluationService, _mapper);
        var act = () => handler.Handle(new GetEvaluationByIdQuery(evaluation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetById_WhenOwner_ShouldReturnDto()
    {
        var userId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), userId, true, 5);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var handler = new GetEvaluationByIdQueryHandler(_currentUserService, _aiEvaluationService, _mapper);
        var result = await handler.Handle(new GetEvaluationByIdQuery(evaluation.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(evaluation.Id);
    }

    [Fact]
    public async Task GetByMessageId_WhenFoundAndOwner_ShouldReturnDto()
    {
        var userId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(messageId, userId, true, 5);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _aiEvaluationService.GetByMessageIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var handler = new GetEvaluationByMessageIdQueryHandler(
            _currentUserService,
            _aiEvaluationService,
            _messageService,
            _conversationService,
            _mapper);

        var result = await handler.Handle(new GetEvaluationByMessageIdQuery(messageId), CancellationToken.None);

        result.Should().NotBeNull();
        result.MessageId.Should().Be(messageId);
    }

    [Fact]
    public async Task GetPaged_WhenUserNotAdmin_ShouldFilterByCallerUserId()
    {
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");

        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), userId, true, 5);
        _aiEvaluationService.GetCountAsync(null, null, userId, Arg.Any<CancellationToken>())
            .Returns(1);
        _aiEvaluationService.GetPagedAsync(0, 10, null, null, userId, Arg.Any<CancellationToken>())
            .Returns(new List<AiEvaluationLog> { evaluation });

        var handler = new GetAiEvaluationsQueryHandler(_currentUserService, _aiEvaluationService, _mapper);
        var query = new GetAiEvaluationsQuery(PageNumber: 1, PageSize: 10, EvaluatorUserId: otherUserId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].EvaluatorUserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetSummary_WhenAuthenticated_ShouldReturnSummaryDto()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _aiEvaluationService.GetSummaryAsync(Arg.Any<CancellationToken>())
            .Returns((TotalCount: 15, AverageRating: 4.6, HelpfulCount: 12, UnhelpfulCount: 3));

        var handler = new GetAiEvaluationSummaryQueryHandler(_currentUserService, _aiEvaluationService);
        var result = await handler.Handle(new GetAiEvaluationSummaryQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalEvaluations.Should().Be(15);
        result.AverageRating.Should().Be(4.6);
        result.HelpfulCount.Should().Be(12);
        result.UnhelpfulCount.Should().Be(3);
    }
}

