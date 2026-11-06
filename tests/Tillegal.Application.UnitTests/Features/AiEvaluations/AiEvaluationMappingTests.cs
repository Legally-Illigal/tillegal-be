using FluentAssertions;
using Mapster;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Models;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class AiEvaluationMappingTests
{
    private readonly IMapper _mapper;

    public AiEvaluationMappingTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_AiEvaluationLog_To_AiEvaluationDto_ShouldMapAllFields()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entity = AiEvaluationLog.Create(
            messageId,
            userId,
            isHelpful: true,
            rating: 5,
            feedbackComment: "Rất chi tiết",
            evaluatorRole: "User");

        // Act
        var dto = _mapper.Map<AiEvaluationDto>(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id);
        dto.MessageId.Should().Be(messageId);
        dto.EvaluatorUserId.Should().Be(userId);
        dto.EvaluatorRole.Should().Be("User");
        dto.IsHelpful.Should().BeTrue();
        dto.Rating.Should().Be(5);
        dto.FeedbackComment.Should().Be("Rất chi tiết");
        dto.CreatedAt.Should().Be(entity.CreatedAt);
    }

    [Fact]
    public void Map_AiEvaluationSummary_To_AiEvaluationSummaryDto_ShouldMapAllFields()
    {
        // Arrange
        var summary = new AiEvaluationSummary(
            TotalEvaluations: 10,
            HelpfulCount: 8,
            UnhelpfulCount: 2,
            AverageRating: 4.2,
            LawyerEvaluationsCount: 4,
            UserEvaluationsCount: 6,
            RatingDistribution: new Dictionary<int, int> { [5] = 6, [4] = 2, [3] = 1, [2] = 1, [1] = 0 });

        // Act
        var dto = _mapper.Map<AiEvaluationSummaryDto>(summary);

        // Assert
        dto.Should().NotBeNull();
        dto.TotalEvaluations.Should().Be(10);
        dto.HelpfulCount.Should().Be(8);
        dto.UnhelpfulCount.Should().Be(2);
        dto.AverageRating.Should().Be(4.2);
        dto.LawyerEvaluationsCount.Should().Be(4);
        dto.UserEvaluationsCount.Should().Be(6);
        dto.RatingDistribution.Should().ContainKey(5).WhoseValue.Should().Be(6);
    }
}

