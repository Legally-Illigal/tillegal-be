using Tillegal.Domain.Entities;
using Tillegal.Domain.Models;

namespace Tillegal.Domain.Interfaces.Repositories;

public interface IAiEvaluationRepository
{
    Task<AiEvaluationLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AiEvaluationLog?> GetByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AiEvaluationLog> Items, int TotalCount)> GetPagedAsync(
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

