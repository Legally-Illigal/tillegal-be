using System.Text.Json;
using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class ConversationMessageTests
{
    [Fact]
    public void CreateUserMessage_WithValidParameters_ShouldReturnMessageWithUserRole()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var content = "Thủ tục xin cấp giấy phép kinh doanh gồm những gì?";

        // Act
        var message = ConversationMessage.CreateUserMessage(conversationId, content);

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
    public void CreateUserMessage_WithEmptyConversationId_ShouldThrowDomainException()
    {
        // Arrange
        var conversationId = Guid.Empty;
        var content = "Câu hỏi hợp lệ";

        // Act
        var act = () => ConversationMessage.CreateUserMessage(conversationId, content);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("ConversationId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void CreateUserMessage_WithNullOrWhitespaceContent_ShouldThrowDomainException(string? invalidContent)
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act
        var act = () => ConversationMessage.CreateUserMessage(conversationId, invalidContent!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Message content cannot be empty.");
    }

    [Fact]
    public void CreateAssistantMessage_WithValidParameters_ShouldReturnMessageWithAssistantRoleAndCitations()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var content = "Hồ sơ gồm đơn đề nghị, điều lệ công ty và danh sách thành viên.";
        var citations = JsonDocument.Parse("""[{"index": 1, "title": "Luật Doanh nghiệp 2020"}]""");

        // Act
        var message = ConversationMessage.CreateAssistantMessage(conversationId, content, citations);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().NotBeEmpty();
        message.ConversationId.Should().Be(conversationId);
        message.Role.Should().Be("assistant");
        message.Content.Should().Be(content);
        message.Citations.Should().NotBeNull();
    }

    [Fact]
    public void CreateAssistantMessage_WithEmptyConversationId_ShouldThrowDomainException()
    {
        // Arrange
        var conversationId = Guid.Empty;
        var content = "Nội dung trả lời";

        // Act
        var act = () => ConversationMessage.CreateAssistantMessage(conversationId, content);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("ConversationId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void CreateAssistantMessage_WithNullOrWhitespaceContent_ShouldThrowDomainException(string? invalidContent)
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act
        var act = () => ConversationMessage.CreateAssistantMessage(conversationId, invalidContent!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Message content cannot be empty.");
    }
}
