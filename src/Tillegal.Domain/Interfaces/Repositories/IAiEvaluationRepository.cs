using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Repositories;

public interface IAiEvaluationRepository
{
    Task<AiEvaluationLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AiEvaluationLog?> GetByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiEvaluationLog>> GetByEvaluatorUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiEvaluationLog>> GetPagedAsync(
        int skip,
        int take,
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default);

    Task<(int TotalCount, double AverageRating, int HelpfulCount, int UnhelpfulCount)> GetSummaryAsync(
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
}

