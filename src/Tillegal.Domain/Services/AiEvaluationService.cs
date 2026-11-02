using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Domain.Services;

public sealed class AiEvaluationService(
    IAiEvaluationRepository evaluationRepository) : IAiEvaluationService
{
    public async Task<AiEvaluationLog> CreateEvaluationAsync(
        Guid messageId,
        Guid? evaluatorUserId,
        bool? isHelpful,
        int? rating,
        string? feedbackComment = null,
        CancellationToken cancellationToken = default)
    {
        var evaluation = AiEvaluationLog.Create(
            messageId,
            evaluatorUserId,
            isHelpful,
            rating,
            feedbackComment);

        await evaluationRepository.AddAsync(evaluation, cancellationToken);
        return evaluation;
    }

    public async Task<AiEvaluationLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<AiEvaluationLog?> GetByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetByMessageIdAsync(messageId, cancellationToken);
    }

    public async Task<IReadOnlyList<AiEvaluationLog>> GetByEvaluatorUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetByEvaluatorUserIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<AiEvaluationLog>> GetPagedAsync(
        int skip,
        int take,
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveTake = take <= 0 ? 10 : Math.Min(take, 100);
        var effectiveSkip = Math.Max(0, skip);

        return await evaluationRepository.GetPagedAsync(
            effectiveSkip,
            effectiveTake,
            isHelpful,
            rating,
            evaluatorUserId,
            cancellationToken);
    }

    public async Task<int> GetCountAsync(
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetCountAsync(
            isHelpful,
            rating,
            evaluatorUserId,
            cancellationToken);
    }

    public async Task<(int TotalCount, double AverageRating, int HelpfulCount, int UnhelpfulCount)> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetSummaryAsync(cancellationToken);
    }

    public async Task<AiEvaluationLog> UpdateEvaluationAsync(
        AiEvaluationLog evaluation,
        bool? isHelpful,
        int? rating,
        string? feedbackComment = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        evaluation.Update(isHelpful, rating, feedbackComment);
        await evaluationRepository.UpdateAsync(evaluation, cancellationToken);
        return evaluation;
    }

    public async Task DeleteEvaluationAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        evaluation.SoftDelete();
        await evaluationRepository.DeleteAsync(evaluation, cancellationToken);
    }
}

