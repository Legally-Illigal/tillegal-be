using FluentAssertions;
using Tillegal.Application.Features.Attachments.Commands.DeleteAttachment;
using Tillegal.Application.Features.Attachments.Commands.UploadAttachment;
using Tillegal.Application.Features.Attachments.Queries.DownloadAttachment;
using Tillegal.Application.Features.Attachments.Queries.GetAttachmentById;
using Tillegal.Application.Features.Attachments.Queries.GetAttachmentsByConversationId;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Attachments;

public sealed class AttachmentCommandValidatorTests
{
    [Fact]
    public void UploadAttachmentCommandValidator_Should_Pass_WhenValid()
    {
        var validator = new UploadAttachmentCommandValidator();
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            "hop_dong.pdf",
            "application/pdf",
            1024,
            stream);

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UploadAttachmentCommandValidator_Should_Fail_WhenConversationIdIsEmpty()
    {
        var validator = new UploadAttachmentCommandValidator();
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(
            Guid.Empty,
            "hop_dong.pdf",
            "application/pdf",
            1024,
            stream);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConversationId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UploadAttachmentCommandValidator_Should_Fail_WhenFileNameIsEmpty(string? fileName)
    {
        var validator = new UploadAttachmentCommandValidator();
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            fileName!,
            "application/pdf",
            1024,
            stream);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileName");
    }

    [Fact]
    public void UploadAttachmentCommandValidator_Should_Fail_WhenFileNameHasUnsupportedExtension()
    {
        var validator = new UploadAttachmentCommandValidator();
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            "virus.exe",
            "application/x-msdownload",
            1024,
            stream);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileName");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(25 * 1024 * 1024)] // > 20 MB
    public void UploadAttachmentCommandValidator_Should_Fail_WhenFileSizeIsInvalid(long fileSize)
    {
        var validator = new UploadAttachmentCommandValidator();
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            "document.pdf",
            "application/pdf",
            fileSize,
            stream);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileSize");
    }

    [Fact]
    public void UploadAttachmentCommandValidator_Should_Fail_WhenContentIsNull()
    {
        var validator = new UploadAttachmentCommandValidator();
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            "document.pdf",
            "application/pdf",
            1024,
            null!);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Content");
    }

    [Fact]
    public void DeleteAttachmentCommandValidator_Should_Pass_WhenValid()
    {
        var validator = new DeleteAttachmentCommandValidator();
        var command = new DeleteAttachmentCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteAttachmentCommandValidator_Should_Fail_WhenIdsAreEmpty()
    {
        var validator = new DeleteAttachmentCommandValidator();
        var command = new DeleteAttachmentCommand(Guid.Empty, Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConversationId");
        result.Errors.Should().Contain(e => e.PropertyName == "AttachmentId");
    }

    [Fact]
    public void GetAttachmentsByConversationIdQueryValidator_Should_ValidateCorrectly()
    {
        var validator = new GetAttachmentsByConversationIdQueryValidator();

        validator.Validate(new GetAttachmentsByConversationIdQuery(Guid.NewGuid())).IsValid.Should().BeTrue();
        validator.Validate(new GetAttachmentsByConversationIdQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetAttachmentByIdQueryValidator_Should_ValidateCorrectly()
    {
        var validator = new GetAttachmentByIdQueryValidator();

        validator.Validate(new GetAttachmentByIdQuery(Guid.NewGuid(), Guid.NewGuid())).IsValid.Should().BeTrue();
        validator.Validate(new GetAttachmentByIdQuery(Guid.Empty, Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void DownloadAttachmentQueryValidator_Should_ValidateCorrectly()
    {
        var validator = new DownloadAttachmentQueryValidator();

        validator.Validate(new DownloadAttachmentQuery(Guid.NewGuid(), Guid.NewGuid())).IsValid.Should().BeTrue();
        validator.Validate(new DownloadAttachmentQuery(Guid.Empty, Guid.Empty)).IsValid.Should().BeFalse();
    }
}

