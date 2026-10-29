using System.Text.Json;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Messages.Models;
using Tillegal.Domain.Entities;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Messages;

public sealed class MessageMappingTests
{
    private readonly IMapper _mapper;

    public MessageMappingTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_ConversationMessage_To_MessageDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var citations = JsonDocument.Parse("""[{"index": 1, "source": "Luật Đầu tư"}]""");
        var message = ConversationMessage.CreateAssistantMessage(conversationId, "Trả lời", citations);

        // Act
        var dto = _mapper.Map<MessageDto>(message);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(message.Id);
        dto.ConversationId.Should().Be(conversationId);
        dto.Role.Should().Be("assistant");
        dto.Content.Should().Be("Trả lời");
        dto.Citations.Should().NotBeNull();
        dto.CreatedAt.Should().Be(message.CreatedAt);
    }
}

