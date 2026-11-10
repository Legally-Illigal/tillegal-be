using MediatR;
using Tillegal.Application.Features.Attachments.Models;

namespace Tillegal.Application.Features.Attachments.Queries.GetAttachmentsByConversationId;

public sealed record GetAttachmentsByConversationIdQuery(Guid ConversationId) : IRequest<IReadOnlyList<AttachmentDto>>;

