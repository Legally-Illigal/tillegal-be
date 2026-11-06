using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationById;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class GetAdminAiEvaluationByIdQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IMapper _mapper;
    private readonly GetAdminAiEvaluationByIdQueryHandler _handler;

    public GetAdminAiEvaluationByIdQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new GetAdminAiEvaluationByIdQueryHandler(
            _currentUserService,
            _aiEvaluationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_AsAdmin_WithExistingEvaluation_ShouldReturnDto()
    {
        // Arrange
        var evalId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var eval = AiEvaluationLog.Create(messageId, userId, true, 5, "Xuất sắc", "Lawyer");
        _aiEvaluationService.GetEvaluationByIdAsync(evalId, Arg.Any<CancellationToken>())
            .Returns(eval);

        var query = new GetAdminAiEvaluationByIdQuery(evalId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Should().Be(messageId);
        result.EvaluatorUserId.Should().Be(userId);
        result.Rating.Should().Be(5);
        result.IsHelpful.Should().BeTrue();
        result.EvaluatorRole.Should().Be("Lawyer");
    }

    [Fact]
    public async Task Handle_AsAdmin_WhenEvaluationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var evalId = Guid.NewGuid();

        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        _aiEvaluationService.GetEvaluationByIdAsync(evalId, Arg.Any<CancellationToken>())
            .Returns((AiEvaluationLog?)null);

        var query = new GetAdminAiEvaluationByIdQuery(evalId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_AsNonAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("User");

        var query = new GetAdminAiEvaluationByIdQuery(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Only administrators can view evaluation management reports*");
    }
}

