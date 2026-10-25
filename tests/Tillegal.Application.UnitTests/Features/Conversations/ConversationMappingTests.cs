using FluentAssertions;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Commands.CreateConversation;
using Tillegal.Domain.Entities;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Conversations;

public sealed class ConversationMappingTests
{
    private readonly IMapper _mapper;

    public ConversationMappingTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_Conversation_To_CreateConversationResult_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại mẫu");

        // Act
        var result = _mapper.Map<CreateConversationResult>(conversation);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(conversation.Id);
        result.UserId.Should().Be(userId);
        result.Title.Should().Be("Hội thoại mẫu");
        result.CreatedAt.Should().Be(conversation.CreatedAt);
        result.UpdatedAt.Should().Be(conversation.UpdatedAt);
    }
}
