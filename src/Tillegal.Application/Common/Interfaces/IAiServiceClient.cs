using Tillegal.Application.Common.Models;

namespace Tillegal.Application.Common.Interfaces;

public interface IAiServiceClient
{
    Task<string?> TriggerCrawlAsync(DateOnly crawlDate, CancellationToken cancellationToken = default);

    Task<AiConsultationResult> ConsultLegalAsync(
        string prompt,
        string workflowMode,
        IReadOnlyList<ConversationHistoryItem>? history = null,
        CancellationToken cancellationToken = default);
}
