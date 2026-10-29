using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class ConversationTests
{
    [Fact]
    public void Create_Should_Initialize_Conversation_WhenValid()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var title = "Tư vấn luật doanh nghiệp";

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
    public void Create_Should_Trim_Title()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var title = "   Tiêu đề cần cắt khoảng trắng   ";

        // Act
        var conversation = Conversation.Create(userId, title);

        // Assert
        conversation.Title.Should().Be("Tiêu đề cần cắt khoảng trắng");
    }

    [Fact]
    public void Create_Should_ThrowDomainException_WhenUserIdIsEmpty()
    {
        // Act
        var act = () => Conversation.Create(Guid.Empty, "Tiêu đề hợp lệ");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("UserId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_ThrowDomainException_WhenTitleIsEmpty(string? title)
    {
        // Act
        var act = () => Conversation.Create(Guid.CreateVersion7(), title!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Title cannot be empty.");
    }

    [Fact]
    public void UpdateTitle_Should_UpdateTitleAndSetUpdatedAt_WhenValid()
    {
        // Arrange
        var conversation = Conversation.Create(Guid.CreateVersion7(), "Tiêu đề cũ");

        // Act
        conversation.UpdateTitle("Tiêu đề mới");

        // Assert
        conversation.Title.Should().Be("Tiêu đề mới");
        conversation.UpdatedAt.Should().NotBeNull();
        conversation.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateTitle_Should_ThrowDomainException_WhenTitleIsEmpty(string? newTitle)
    {
        // Arrange
        var conversation = Conversation.Create(Guid.CreateVersion7(), "Tiêu đề cũ");

        // Act
        var act = () => conversation.UpdateTitle(newTitle!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Title cannot be empty.");
    }

    [Fact]
    public void SoftDelete_Should_SetDeletedAtAndUpdatedAt()
    {
        // Arrange
        var conversation = Conversation.Create(Guid.CreateVersion7(), "Tiêu đề");

        // Act
        conversation.SoftDelete();

        // Assert
        conversation.DeletedAt.Should().NotBeNull();
        conversation.UpdatedAt.Should().NotBeNull();
        conversation.DeletedAt.Should().Be(conversation.UpdatedAt);
    }
}
