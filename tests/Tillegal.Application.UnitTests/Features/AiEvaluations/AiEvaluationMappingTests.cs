using FluentAssertions;
using Mapster;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Entities;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class AiEvaluationMappingTests
{
    private readonly IMapper _mapper;

    public AiEvaluationMappingTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_AiEvaluationLog_To_AiEvaluationDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var messageId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(
            messageId,
            userId,
            isHelpful: true,
            rating: 5,
            feedbackComment: "Rất chi tiết và chính xác.");

        // Act
        var dto = _mapper.Map<AiEvaluationDto>(evaluation);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(evaluation.Id);
        dto.MessageId.Should().Be(messageId);
        dto.EvaluatorUserId.Should().Be(userId);
        dto.IsHelpful.Should().BeTrue();
        dto.Rating.Should().Be(5);
        dto.FeedbackComment.Should().Be("Rất chi tiết và chính xác.");
        dto.CreatedAt.Should().Be(evaluation.CreatedAt);
    }
}

