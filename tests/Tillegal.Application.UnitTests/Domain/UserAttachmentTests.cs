using FluentAssertions;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class UserAttachmentTests
{
    [Fact]
    public void Create_Should_SetPropertiesCorrectly_WhenArgumentsAreValid()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileName = "hop_dong.pdf";
        var storagePath = "attachments/user/hop_dong.pdf";
        var fileSize = 1024L;
        var contentType = "application/pdf";

        // Act
        var attachment = UserAttachment.Create(
            conversationId,
            userId,
            fileName,
            storagePath,
            fileSize,
            contentType);

        // Assert
        attachment.Id.Should().NotBeEmpty();
        attachment.ConversationId.Should().Be(conversationId);
        attachment.UserId.Should().Be(userId);
        attachment.FileName.Should().Be(fileName);
        attachment.StoragePath.Should().Be(storagePath);
        attachment.FileSize.Should().Be(fileSize);
        attachment.ContentType.Should().Be(contentType);
        attachment.ProcessingStatus.Should().Be("Pending");
        attachment.TokenCount.Should().BeNull();
        attachment.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_Should_ThrowDomainException_WhenConversationIdIsEmpty()
    {
        var act = () => UserAttachment.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        act.Should().Throw<DomainException>().WithMessage("*ConversationId*");
    }

    [Fact]
    public void Create_Should_ThrowDomainException_WhenUserIdIsEmpty()
    {
        var act = () => UserAttachment.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        act.Should().Throw<DomainException>().WithMessage("*UserId*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_WhenFileNameIsInvalid(string fileName)
    {
        var act = () => UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            fileName,
            "path/file.pdf",
            100,
            "application/pdf");

        act.Should().Throw<DomainException>().WithMessage("*FileName*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_WhenStoragePathIsInvalid(string storagePath)
    {
        var act = () => UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            storagePath,
            100,
            "application/pdf");

        act.Should().Throw<DomainException>().WithMessage("*StoragePath*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Should_ThrowDomainException_WhenFileSizeIsNotGreaterThanZero(long fileSize)
    {
        var act = () => UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            fileSize,
            "application/pdf");

        act.Should().Throw<DomainException>().WithMessage("*FileSize*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_WhenContentTypeIsInvalid(string contentType)
    {
        var act = () => UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            contentType);

        act.Should().Throw<DomainException>().WithMessage("*ContentType*");
    }

    [Fact]
    public void UpdateProcessingStatus_Should_UpdateStatusAndTokenCount()
    {
        var attachment = UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        attachment.UpdateProcessingStatus("Completed", 1250);

        attachment.ProcessingStatus.Should().Be("Completed");
        attachment.TokenCount.Should().Be(1250);
        attachment.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProcessingStatus_Should_ThrowDomainException_WhenStatusIsEmpty(string status)
    {
        var attachment = UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        var act = () => attachment.UpdateProcessingStatus(status);

        act.Should().Throw<DomainException>().WithMessage("*ProcessingStatus*");
    }

    [Fact]
    public void SoftDelete_Should_SetDeletedAt()
    {
        var attachment = UserAttachment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "file.pdf",
            "path/file.pdf",
            100,
            "application/pdf");

        attachment.SoftDelete();

        attachment.DeletedAt.Should().NotBeNull();
    }
}

