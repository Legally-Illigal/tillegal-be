using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluations;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class GetAdminAiEvaluationsQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IMapper _mapper;
    private readonly GetAdminAiEvaluationsQueryHandler _handler;

    public GetAdminAiEvaluationsQueryHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new GetAdminAiEvaluationsQueryHandler(
            _currentUserService,
            _aiEvaluationService,
            _mapper);
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldReturnPagedResults()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var items = new List<AiEvaluationLog>
        {
            AiEvaluationLog.Create(Guid.NewGuid(), Guid.NewGuid(), true, 5, "Good", "User"),
            AiEvaluationLog.Create(Guid.NewGuid(), Guid.NewGuid(), false, 2, "Poor", "Lawyer")
        };

        _aiEvaluationService.GetPagedEvaluationsAsync(
            1, 10, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 2));

        var query = new GetAdminAiEvaluationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AsNonAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("User");

        var query = new GetAdminAiEvaluationsQuery();

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*Only administrators can view evaluation management reports*");
    }
}

