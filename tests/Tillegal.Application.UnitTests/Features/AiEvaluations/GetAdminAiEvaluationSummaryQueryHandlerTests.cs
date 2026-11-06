using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationSummary;
using Tillegal.Domain.Interfaces.Services;
using Tillegal.Domain.Models;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class GetAdminAiEvaluationSummaryQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IMapper _mapper;
    private readonly GetAdminAiEvaluationSummaryQueryHandler _handler;

    public GetAdminAiEvaluationSummaryQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new GetAdminAiEvaluationSummaryQueryHandler(
            _currentUserService,
            _aiEvaluationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldReturnSummary()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var summary = new AiEvaluationSummary(
            TotalEvaluations: 15,
            HelpfulCount: 12,
            UnhelpfulCount: 3,
            AverageRating: 4.5,
            LawyerEvaluationsCount: 6,
            UserEvaluationsCount: 9,
            RatingDistribution: new Dictionary<int, int> { [5] = 10, [4] = 2, [3] = 1, [2] = 2, [1] = 0 });

        _aiEvaluationService.GetSummaryAsync(Arg.Any<CancellationToken>())
            .Returns(summary);

        var query = new GetAdminAiEvaluationSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalEvaluations.Should().Be(15);
        result.HelpfulCount.Should().Be(12);
        result.UnhelpfulCount.Should().Be(3);
        result.AverageRating.Should().Be(4.5);
        result.LawyerEvaluationsCount.Should().Be(6);
        result.UserEvaluationsCount.Should().Be(9);
    }

    [Fact]
    public async Task Handle_AsNonAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("User");

        var query = new GetAdminAiEvaluationSummaryQuery();

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Only administrators can view evaluation management reports*");
    }
}

