using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class ConversationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnConversationWithGuidV7()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Tư vấn thủ tục doanh nghiệp";

        // Act
        var conversation = Conversation.Create(userId, title);

        // Assert
        conversation.Should().NotBeNull();
        conversation.Id.Should().NotBeEmpty();
        conversation.UserId.Should().Be(userId);
        conversation.Title.Should().Be(title);
        conversation.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        conversation.UpdatedAt.Should().BeNull();
        conversation.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.Empty;
        var title = "Valid Title";

        // Act
        var act = () => Conversation.Create(userId, title);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("UserId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Create_WithNullOrWhitespaceTitle_ShouldThrowDomainException(string? invalidTitle)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Conversation.Create(userId, invalidTitle!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Title cannot be empty.");
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitleAndMarkUpdated()
    {
        // Arrange
        var conversation = Conversation.Create(Guid.NewGuid(), "Old Title");
        var newTitle = "Updated New Title";

        // Act
        conversation.UpdateTitle(newTitle);

        // Assert
        conversation.Title.Should().Be(newTitle);
        conversation.UpdatedAt.Should().NotBeNull();
        conversation.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void UpdateTitle_WithNullOrWhitespaceTitle_ShouldThrowDomainException(string? invalidTitle)
    {
        // Arrange
        var conversation = Conversation.Create(Guid.NewGuid(), "Valid Title");

        // Act
        var act = () => conversation.UpdateTitle(invalidTitle!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Title cannot be empty.");
    }
}
