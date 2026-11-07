using MediatR;
using Tillegal.Application.Features.Attachments.Models;

namespace Tillegal.Application.Features.Attachments.Queries.GetAttachmentById;

public sealed record GetAttachmentByIdQuery(Guid ConversationId, Guid AttachmentId) : IRequest<AttachmentDto>;

