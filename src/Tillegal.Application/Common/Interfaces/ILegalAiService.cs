using System.Text.Json;

namespace Tillegal.Application.Common.Interfaces;

public interface ILegalAiService
{
    Task<AiLegalAnswer> AskAsync(
        AiQuestionContext context,
        CancellationToken cancellationToken = default);
}

public sealed record AiQuestionContext(
    Guid ConversationId,
    string Question,
    IReadOnlyList<AiMessageContext>? History = null);

public sealed record AiMessageContext(
    string Role,
    string Content);

public sealed record AiLegalAnswer(
    string Content,
    JsonDocument? Citations = null);
