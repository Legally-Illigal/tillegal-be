using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class AiEvaluationLogTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnEvaluation()
    {
        // Arrange
        var messageId = Guid.CreateVersion7();
        var evaluatorUserId = Guid.CreateVersion7();

        // Act
        var evaluation = AiEvaluationLog.Create(
            messageId,
            evaluatorUserId,
            isHelpful: true,
            rating: 5,
            feedbackComment: "  Rất chính xác và hữu ích.  ");

        // Assert
        evaluation.Should().NotBeNull();
        evaluation.Id.Should().NotBeEmpty();
        evaluation.MessageId.Should().Be(messageId);
        evaluation.EvaluatorUserId.Should().Be(evaluatorUserId);
        evaluation.IsHelpful.Should().BeTrue();
        evaluation.Rating.Should().Be(5);
        evaluation.FeedbackComment.Should().Be("Rất chính xác và hữu ích.");
        evaluation.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        evaluation.UpdatedAt.Should().BeNull();
        evaluation.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyMessageId_ShouldThrowDomainException()
    {
        // Act
        var act = () => AiEvaluationLog.Create(
            Guid.Empty,
            Guid.CreateVersion7(),
            isHelpful: true,
            rating: 4);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("MessageId cannot be empty.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithInvalidRating_ShouldThrowDomainException(int invalidRating)
    {
        // Act
        var act = () => AiEvaluationLog.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            isHelpful: true,
            rating: invalidRating);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNoAttributes_ShouldThrowDomainException(string? emptyComment)
    {
        // Act
        var act = () => AiEvaluationLog.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            isHelpful: null,
            rating: null,
            feedbackComment: emptyComment);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("At least one evaluation attribute (isHelpful, rating, or feedbackComment) must be provided.");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAndMarkUpdated()
    {
        // Arrange
        var evaluation = AiEvaluationLog.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            isHelpful: false,
            rating: 2,
            feedbackComment: "Cần cải thiện");

        // Act
        evaluation.Update(
            isHelpful: true,
            rating: 4,
            feedbackComment: "Đã hiểu rõ hơn");

        // Assert
        evaluation.IsHelpful.Should().BeTrue();
        evaluation.Rating.Should().Be(4);
        evaluation.FeedbackComment.Should().Be("Đã hiểu rõ hơn");
        evaluation.UpdatedAt.Should().NotBeNull();
        evaluation.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Update_WithInvalidRating_ShouldThrowDomainException(int invalidRating)
    {
        // Arrange
        var evaluation = AiEvaluationLog.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            isHelpful: true,
            rating: 5);

        // Act
        var act = () => evaluation.Update(isHelpful: true, rating: invalidRating, feedbackComment: null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5.");
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndUpdatedAt()
    {
        // Arrange
        var evaluation = AiEvaluationLog.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            isHelpful: true,
            rating: 5);

        // Act
        evaluation.SoftDelete();

        // Assert
        evaluation.DeletedAt.Should().NotBeNull();
        evaluation.UpdatedAt.Should().NotBeNull();
        evaluation.DeletedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        var evaluatorUserId = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var deletedAt = DateTimeOffset.UtcNow;

        // Act
        var evaluation = new AiEvaluationLog(
            id,
            messageId,
            evaluatorUserId,
            isHelpful: true,
            rating: 4,
            feedbackComment: "Tốt",
            createdAt,
            updatedAt,
            deletedAt);

        // Assert
        evaluation.Id.Should().Be(id);
        evaluation.MessageId.Should().Be(messageId);
        evaluation.EvaluatorUserId.Should().Be(evaluatorUserId);
        evaluation.IsHelpful.Should().BeTrue();
        evaluation.Rating.Should().Be(4);
        evaluation.FeedbackComment.Should().Be("Tốt");
        evaluation.CreatedAt.Should().Be(createdAt);
        evaluation.UpdatedAt.Should().Be(updatedAt);
        evaluation.DeletedAt.Should().Be(deletedAt);
    }
}

