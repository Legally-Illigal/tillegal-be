using MediatR;
using Tillegal.Application.Features.Attachments.Models;

namespace Tillegal.Application.Features.Attachments.Commands.UploadAttachment;

public sealed record UploadAttachmentCommand(
    Guid ConversationId,
    string FileName,
    string ContentType,
    long FileSize,
    Stream Content) : IRequest<AttachmentDto>;

