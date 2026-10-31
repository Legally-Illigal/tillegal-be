using System.Text.Json;
using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class ConversationMessageTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnMessage()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var content = "Nội dung câu hỏi pháp lý";

        // Act
        var message = ConversationMessage.Create(conversationId, "user", content);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().NotBeEmpty();
        message.ConversationId.Should().Be(conversationId);
        message.Role.Should().Be("user");
        message.Content.Should().Be(content);
        message.Citations.Should().BeNull();
        message.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        message.UpdatedAt.Should().BeNull();
        message.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyConversationId_ShouldThrowDomainException()
    {
        // Act
        var act = () => ConversationMessage.Create(Guid.Empty, "user", "Nội dung");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("ConversationId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Create_WithNullOrWhitespaceRole_ShouldThrowDomainException(string? invalidRole)
    {
        // Act
        var act = () => ConversationMessage.Create(Guid.CreateVersion7(), invalidRole!, "Nội dung");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Role cannot be empty.");
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("invalid_role")]
    [InlineData("moderator")]
    public void Create_WithInvalidRole_ShouldThrowDomainException(string invalidRole)
    {
        // Act
        var act = () => ConversationMessage.Create(Guid.CreateVersion7(), invalidRole, "Nội dung");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"Invalid message role: '{invalidRole}'. Allowed roles are 'user', 'assistant', 'system'.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Create_WithNullOrWhitespaceContent_ShouldThrowDomainException(string? invalidContent)
    {
        // Act
        var act = () => ConversationMessage.Create(Guid.CreateVersion7(), "user", invalidContent!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Message content cannot be empty.");
    }

    [Fact]
    public void CreateUserMessage_WithValidParameters_ShouldReturnMessageWithUserRole()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var content = "Thủ tục xin cấp giấy phép kinh doanh gồm những gì?";

        // Act
        var message = ConversationMessage.CreateUserMessage(conversationId, content);

        // Assert
        message.Should().NotBeNull();
        message.ConversationId.Should().Be(conversationId);
        message.Role.Should().Be("user");
        message.Content.Should().Be(content);
        message.Citations.Should().BeNull();
    }

    [Fact]
    public void CreateAssistantMessage_WithValidParameters_ShouldReturnMessageWithAssistantRoleAndCitations()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var content = "Hồ sơ gồm đơn đề nghị, điều lệ công ty và danh sách thành viên.";
        var citations = JsonDocument.Parse("""[{"index": 1, "title": "Luật Doanh nghiệp 2020"}]""");

        // Act
        var message = ConversationMessage.CreateAssistantMessage(conversationId, content, citations);

        // Assert
        message.Should().NotBeNull();
        message.ConversationId.Should().Be(conversationId);
        message.Role.Should().Be("assistant");
        message.Content.Should().Be(content);
        message.Citations.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContent_WithValidContent_ShouldUpdateContentAndSetUpdatedAt()
    {
        // Arrange
        var message = ConversationMessage.CreateUserMessage(Guid.CreateVersion7(), "Nội dung cũ");
        var citations = JsonDocument.Parse("""{"source": "test"}""");

        // Act
        message.UpdateContent("Nội dung mới đã cập nhật", citations);

        // Assert
        message.Content.Should().Be("Nội dung mới đã cập nhật");
        message.Citations.Should().NotBeNull();
        message.UpdatedAt.Should().NotBeNull();
        message.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void UpdateContent_WithNullOrWhitespaceContent_ShouldThrowDomainException(string? invalidContent)
    {
        // Arrange
        var message = ConversationMessage.CreateUserMessage(Guid.CreateVersion7(), "Nội dung cũ");

        // Act
        var act = () => message.UpdateContent(invalidContent!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Message content cannot be empty.");
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndUpdatedAt()
    {
        // Arrange
        var message = ConversationMessage.CreateUserMessage(Guid.CreateVersion7(), "Nội dung");

        // Act
        message.SoftDelete();

        // Assert
        message.DeletedAt.Should().NotBeNull();
        message.UpdatedAt.Should().NotBeNull();
        message.DeletedAt.Should().Be(message.UpdatedAt);
    }
}
