using Tillegal.Domain.Entities;
using Tillegal.Domain.Models;

namespace Tillegal.Domain.Interfaces.Services;

public interface IAiEvaluationService
{
    Task<AiEvaluationLog> CreateEvaluationAsync(
        Guid messageId,
        Guid? evaluatorUserId,
        bool? isHelpful,
        int? rating,
        string? feedbackComment = null,
        string? evaluatorRole = null,
        CancellationToken cancellationToken = default);

    Task<AiEvaluationLog> UpdateEvaluationAsync(
        AiEvaluationLog evaluation,
        bool? isHelpful,
        int? rating,
        string? feedbackComment = null,
        CancellationToken cancellationToken = default);

    Task DeleteEvaluationAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default);

    Task<AiEvaluationLog?> GetEvaluationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AiEvaluationLog?> GetEvaluationByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AiEvaluationLog> Items, int TotalCount)> GetPagedEvaluationsAsync(
        int pageNumber,
        int pageSize,
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        string? evaluatorRole = null,
        CancellationToken cancellationToken = default);

    Task<AiEvaluationSummary> GetSummaryAsync(
        CancellationToken cancellationToken = default);
}

