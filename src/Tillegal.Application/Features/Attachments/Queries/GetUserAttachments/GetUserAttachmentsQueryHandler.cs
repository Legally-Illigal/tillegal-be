using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Attachments.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Attachments.Queries.GetUserAttachments;

public sealed class GetUserAttachmentsQueryHandler(
    ICurrentUserService currentUserService,
    IUserAttachmentService attachmentService,
    IMapper mapper) : IRequestHandler<GetUserAttachmentsQuery, IReadOnlyList<AttachmentDto>>
{
    public async Task<IReadOnlyList<AttachmentDto>> Handle(
        GetUserAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            throw new ForbiddenAccessException("Admin is not allowed to access user files.");
        }

        var attachments = await attachmentService.GetAttachmentsByUserIdAsync(
            userId,
            cancellationToken);

        return mapper.Map<IReadOnlyList<AttachmentDto>>(attachments);
    }
}

