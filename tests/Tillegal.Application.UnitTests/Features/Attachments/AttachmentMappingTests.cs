using FluentAssertions;
using Mapster;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Attachments.Mappings;
using Tillegal.Application.Features.Attachments.Models;
using Tillegal.Domain.Entities;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Attachments;

public sealed class AttachmentMappingTests
{
    private readonly IMapper _mapper;

    public AttachmentMappingTests()
    {
        var config = MappingConfiguration.Create();
        new AttachmentMappingConfig().Register(config);
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Should_Map_UserAttachment_To_AttachmentDto()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var attachment = UserAttachment.Create(
            conversationId,
            userId,
            "hop_dong.pdf",
            "path/hop_dong.pdf",
            2048,
            "application/pdf");

        var dto = _mapper.Map<AttachmentDto>(attachment);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(attachment.Id);
        dto.ConversationId.Should().Be(conversationId);
        dto.UserId.Should().Be(userId);
        dto.FileName.Should().Be("hop_dong.pdf");
        dto.StoragePath.Should().Be("path/hop_dong.pdf");
        dto.FileSize.Should().Be(2048);
        dto.ContentType.Should().Be("application/pdf");
        dto.ProcessingStatus.Should().Be("Pending");
    }
}

