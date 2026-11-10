using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Attachments.Commands.DeleteAttachment;

public sealed class DeleteAttachmentCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IUserAttachmentService attachmentService,
    IStorageService storageService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteAttachmentCommand>
{
    public async Task Handle(
        DeleteAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            throw new ForbiddenAccessException("Admin is not allowed to upload or manage user files.");
        }

        var conversation = await conversationService.GetConversationByIdAsync(
            request.ConversationId,
            cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenAccessException("You do not have access to this conversation.");
        }

        var attachment = await attachmentService.GetAttachmentByIdAsync(
            request.AttachmentId,
            cancellationToken);

        if (attachment is null || attachment.ConversationId != request.ConversationId)
        {
            throw new NotFoundException("UserAttachment", request.AttachmentId);
        }

        if (attachment.UserId != userId)
        {
            throw new ForbiddenAccessException("You do not have access to this file.");
        }

        try
        {
            await storageService.DeleteAsync(attachment.StoragePath, cancellationToken);
        }
        catch
        {
            // Storage file might already be removed or missing; continue to clean up record.
        }

        await attachmentService.DeleteAttachmentAsync(attachment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

