using FluentAssertions;
using NSubstitute;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class UserAttachmentServiceTests
{
    private readonly IUserAttachmentRepository _repository = Substitute.For<IUserAttachmentRepository>();
    private readonly UserAttachmentService _service;

    public UserAttachmentServiceTests()
    {
        _service = new UserAttachmentService(_repository);
    }

    [Fact]
    public async Task DeleteAttachmentAsync_WhenStatusIsPending_ShouldHardDeleteFromRepository()
    {
        // Arrange
        var attachment = UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        attachment.ProcessingStatus.Should().Be("Pending");

        // Act
        await _service.DeleteAttachmentAsync(attachment, CancellationToken.None);

        // Assert
        await _repository.Received(1).HardDeleteAsync(attachment, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteAsync(attachment, Arg.Any<CancellationToken>());
        attachment.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAttachmentAsync_WhenStatusIsCompleted_ShouldSoftDeleteFromRepository()
    {
        // Arrange
        var attachment = UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        attachment.UpdateProcessingStatus("Completed", 500);

        // Act
        await _service.DeleteAttachmentAsync(attachment, CancellationToken.None);

        // Assert
        attachment.DeletedAt.Should().NotBeNull();
        await _repository.Received(1).DeleteAsync(attachment, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().HardDeleteAsync(attachment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAttachmentAsync_ShouldCallAddAsyncAndReturnAttachment()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileHash = "abcdef1234567890";

        var result = await _service.CreateAttachmentAsync(
            conversationId,
            userId,
            "doc.pdf",
            "storage/doc.pdf",
            1024,
            "application/pdf",
            fileHash,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.ConversationId.Should().Be(conversationId);
        result.UserId.Should().Be(userId);
        result.FileName.Should().Be("doc.pdf");
        result.FileHash.Should().Be(fileHash);
        await _repository.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAttachmentByIdAsync_ShouldDelegateToRepository()
    {
        var id = Guid.NewGuid();
        var attachment = UserAttachment.Create(Guid.NewGuid(), Guid.NewGuid(), "doc.pdf", "path", 100, "application/pdf");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(attachment);

        var result = await _service.GetAttachmentByIdAsync(id, CancellationToken.None);

        result.Should().Be(attachment);
        await _repository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAttachmentsByConversationIdAsync_ShouldDelegateToRepository()
    {
        var conversationId = Guid.NewGuid();
        var attachments = new List<UserAttachment>
        {
            UserAttachment.Create(conversationId, Guid.NewGuid(), "doc.pdf", "path", 100, "application/pdf")
        };
        _repository.GetByConversationIdAsync(conversationId, Arg.Any<CancellationToken>()).Returns(attachments);

        var result = await _service.GetAttachmentsByConversationIdAsync(conversationId, CancellationToken.None);

        result.Should().BeEquivalentTo(attachments);
        await _repository.Received(1).GetByConversationIdAsync(conversationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAttachmentsByUserIdAsync_ShouldDelegateToRepository()
    {
        var userId = Guid.NewGuid();
        var attachments = new List<UserAttachment>
        {
            UserAttachment.Create(Guid.NewGuid(), userId, "doc.pdf", "path", 100, "application/pdf")
        };
        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(attachments);

        var result = await _service.GetAttachmentsByUserIdAsync(userId, CancellationToken.None);

        result.Should().BeEquivalentTo(attachments);
        await _repository.Received(1).GetByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }
}

