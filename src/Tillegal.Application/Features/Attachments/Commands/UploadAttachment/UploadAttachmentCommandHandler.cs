using System.Security.Cryptography;
using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Attachments.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Attachments.Commands.UploadAttachment;

public sealed class UploadAttachmentCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IUserAttachmentService attachmentService,
    IStorageService storageService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UploadAttachmentCommand, AttachmentDto>
{
    private const int MaxAttachmentsPerConversation = 5;

    public async Task<AttachmentDto> Handle(
        UploadAttachmentCommand request,
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

        var existingAttachments = await attachmentService.GetAttachmentsByConversationIdAsync(
            request.ConversationId,
            cancellationToken);

        if (existingAttachments.Count >= MaxAttachmentsPerConversation)
        {
            throw new DomainException($"A conversation can have at most {MaxAttachmentsPerConversation} attachments.");
        }

        Stream streamToUpload = request.Content;
        MemoryStream? memoryStream = null;
        if (!request.Content.CanSeek)
        {
            memoryStream = new MemoryStream();
            await request.Content.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            streamToUpload = memoryStream;
        }

        try
        {
            if (streamToUpload.CanSeek)
            {
                streamToUpload.Position = 0;
            }

            var hashBytes = await SHA256.HashDataAsync(streamToUpload, cancellationToken);
            var fileHash = Convert.ToHexStringLower(hashBytes);

            if (streamToUpload.CanSeek)
            {
                streamToUpload.Position = 0;
            }

            var isDuplicateContent = existingAttachments.Any(a =>
                !string.IsNullOrEmpty(a.FileHash) && string.Equals(a.FileHash, fileHash, StringComparison.OrdinalIgnoreCase));

            if (isDuplicateContent)
            {
                throw new DomainException("This file has already been uploaded to this conversation.");
            }

            var rawFileName = Path.GetFileName(request.FileName);
            var sanitizedFileName = string.Concat(rawFileName.Split(Path.GetInvalidFileNameChars())).Trim();
            if (string.IsNullOrWhiteSpace(sanitizedFileName))
            {
                sanitizedFileName = "attachment";
            }

            var finalFileName = sanitizedFileName;
            var existingNames = existingAttachments.Select(a => a.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (existingNames.Contains(finalFileName))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitizedFileName);
                var ext = Path.GetExtension(sanitizedFileName);
                var counter = 2;
                while (existingNames.Contains($"{nameWithoutExt} ({counter}){ext}"))
                {
                    counter++;
                }
                finalFileName = $"{nameWithoutExt} ({counter}){ext}";
            }

            var fileId = Guid.NewGuid();
            var storagePath = $"attachments/{userId}/{fileId:N}/{finalFileName}";

            await storageService.UploadAsync(
                storagePath,
                streamToUpload,
                request.ContentType,
                cancellationToken);

            var attachment = await attachmentService.CreateAttachmentAsync(
                request.ConversationId,
                userId,
                finalFileName,
                storagePath,
                request.FileSize,
                request.ContentType,
                fileHash,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<AttachmentDto>(attachment);
        }
        finally
        {
            memoryStream?.Dispose();
        }
    }
}
