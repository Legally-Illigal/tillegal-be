using MediatR;
using Tillegal.Application.Features.Attachments.Models;

namespace Tillegal.Application.Features.Attachments.Queries.DownloadAttachment;

public sealed record DownloadAttachmentQuery(Guid ConversationId, Guid AttachmentId) : IRequest<AttachmentDownloadDto>;

