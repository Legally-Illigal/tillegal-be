using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Conversations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    public async Task<ConversationDto> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? "Cuộc hội thoại mới"
            : request.Title.Trim();

        var conversation = await conversationService.CreateConversationAsync(
            userId,
            title,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ConversationDto>(conversation);
    }
}
