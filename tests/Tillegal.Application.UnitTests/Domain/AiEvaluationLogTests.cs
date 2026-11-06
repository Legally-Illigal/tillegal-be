using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class AiEvaluationLogTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var evaluation = AiEvaluationLog.Create(
            messageId,
            userId,
            isHelpful: true,
            rating: 5,
            feedbackComment: "Rất chi tiết và dễ hiểu",
            evaluatorRole: "User");

        // Assert
        evaluation.Should().NotBeNull();
        evaluation.Id.Should().NotBeEmpty();
        evaluation.MessageId.Should().Be(messageId);
        evaluation.EvaluatorUserId.Should().Be(userId);
        evaluation.IsHelpful.Should().BeTrue();
        evaluation.Rating.Should().Be(5);
        evaluation.FeedbackComment.Should().Be("Rất chi tiết và dễ hiểu");
        evaluation.EvaluatorRole.Should().Be("User");
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
            Guid.NewGuid(),
            isHelpful: true,
            rating: 4);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*MessageId cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithInvalidRating_ShouldThrowDomainException(int invalidRating)
    {
        // Act
        var act = () => AiEvaluationLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            isHelpful: null,
            rating: invalidRating);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Rating must be between 1 and 5*");
    }

    [Fact]
    public void Create_WithoutAnyAttributes_ShouldThrowDomainException()
    {
        // Act
        var act = () => AiEvaluationLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            isHelpful: null,
            rating: null,
            feedbackComment: null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*At least one evaluation attribute*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdatePropertiesAndMarkUpdated()
    {
        // Arrange
        var evaluation = AiEvaluationLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            isHelpful: true,
            rating: 4,
            feedbackComment: "Ban đầu khá ổn");

        // Act
        evaluation.Update(
            isHelpful: false,
            rating: 2,
            feedbackComment: "Đọc lại thấy chưa chuẩn");

        // Assert
        evaluation.IsHelpful.Should().BeFalse();
        evaluation.Rating.Should().Be(2);
        evaluation.FeedbackComment.Should().Be("Đọc lại thấy chưa chuẩn");
        evaluation.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAt()
    {
        // Arrange
        var evaluation = AiEvaluationLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            isHelpful: true,
            rating: 5);

        // Act
        evaluation.SoftDelete();

        // Assert
        evaluation.DeletedAt.Should().NotBeNull();
        evaluation.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }
}

