using System.Text.Json;
using MediatR;

namespace Tillegal.Application.Features.Conversations.Commands.AskQuestion;

public sealed record AskQuestionCommand(
    Guid ConversationId,
    string Question) : IRequest<AskQuestionResult>;

public sealed record ConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    JsonDocument? Citations,
    DateTimeOffset CreatedAt);

public sealed record AskQuestionResult(
    ConversationMessageDto UserMessage,
    ConversationMessageDto AssistantMessage);
