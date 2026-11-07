namespace Tillegal.Application.Features.Attachments.Models;

public sealed record AttachmentDownloadDto(
    byte[] Content,
    string ContentType,
    string FileName);

