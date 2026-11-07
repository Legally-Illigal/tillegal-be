using FluentValidation;

namespace Tillegal.Application.Features.Attachments.Commands.UploadAttachment;

public sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    private static readonly string[] AllowedExtensions =
    [
        ".pdf", ".docx", ".doc", ".txt", ".png", ".jpg", ".jpeg", ".webp"
    ];

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId cannot be empty.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("FileName cannot be empty.")
            .MaximumLength(255)
            .WithMessage("FileName cannot exceed 255 characters.")
            .Must(HaveAllowedExtension)
            .WithMessage("File type is not supported. Allowed formats: PDF, DOCX, DOC, TXT, PNG, JPG, JPEG, WEBP.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("ContentType cannot be empty.")
            .MaximumLength(100)
            .WithMessage("ContentType cannot exceed 100 characters.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("FileSize must be greater than zero.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("FileSize cannot exceed 20 MB.");

        RuleFor(x => x.Content)
            .NotNull()
            .WithMessage("File content cannot be null.");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension)
            && AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}

