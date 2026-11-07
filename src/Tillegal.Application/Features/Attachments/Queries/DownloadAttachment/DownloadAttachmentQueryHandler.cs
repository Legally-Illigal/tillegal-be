using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Attachments.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Attachments.Queries.DownloadAttachment;

public sealed class DownloadAttachmentQueryHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IUserAttachmentService attachmentService,
    IStorageService storageService) : IRequestHandler<DownloadAttachmentQuery, AttachmentDownloadDto>
{
    public async Task<AttachmentDownloadDto> Handle(
        DownloadAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            throw new ForbiddenAccessException("Admin is not allowed to access user files.");
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

        var storedObject = await storageService.DownloadAsync(attachment.StoragePath, cancellationToken);
        if (storedObject is null)
        {
            throw new NotFoundException("Stored file for attachment", request.AttachmentId);
        }

        return new AttachmentDownloadDto(storedObject.Content, attachment.ContentType, attachment.FileName);
    }
}

