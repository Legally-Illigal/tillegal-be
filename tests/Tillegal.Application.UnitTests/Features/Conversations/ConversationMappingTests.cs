using FluentAssertions;
using Mapster;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Conversations.Models;
using Tillegal.Domain.Entities;

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
    public void Map_Conversation_To_ConversationDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var conversation = new Conversation(
            id,
            userId,
            "Tiêu đề test",
            createdAt,
            updatedAt,
            null);

        // Act
        var dto = _mapper.Map<ConversationDto>(conversation);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.UserId.Should().Be(userId);
        dto.Title.Should().Be("Tiêu đề test");
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
