using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Services;

namespace Tillegal.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationCommandHandler(
    ICurrentUserService currentUserService,
    ConversationService conversationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateConversationCommand, CreateConversationResult>
{
    private const string DefaultTitle = "Cuộc hội thoại mới";

    public async Task<CreateConversationResult> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? DefaultTitle
            : request.Title.Trim();

        var conversation = await conversationService.CreateConversationAsync(
            userId,
            title,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CreateConversationResult>(conversation);
    }
}
