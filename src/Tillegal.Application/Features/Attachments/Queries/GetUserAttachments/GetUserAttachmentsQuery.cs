using MediatR;
using Tillegal.Application.Features.Attachments.Models;

namespace Tillegal.Application.Features.Attachments.Queries.GetUserAttachments;

public sealed record GetUserAttachmentsQuery : IRequest<IReadOnlyList<AttachmentDto>>;

