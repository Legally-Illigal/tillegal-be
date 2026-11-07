using MediatR;

namespace Tillegal.Application.Features.Attachments.Commands.DeleteAttachment;

public sealed record DeleteAttachmentCommand(Guid ConversationId, Guid AttachmentId) : IRequest;

