using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Interfaces.Services;
using Tillegal.Domain.Models;

namespace Tillegal.Domain.Services;

public sealed class AiEvaluationService(IAiEvaluationRepository evaluationRepository) : IAiEvaluationService
{
    public async Task<AiEvaluationLog> CreateEvaluationAsync(
        Guid messageId,
        Guid? evaluatorUserId,
        bool? isHelpful,
        int? rating,
        string? feedbackComment = null,
        string? evaluatorRole = null,
        CancellationToken cancellationToken = default)
    {
        var evaluation = AiEvaluationLog.Create(
            messageId,
            evaluatorUserId,
            isHelpful,
            rating,
            feedbackComment,
            evaluatorRole);

        await evaluationRepository.AddAsync(evaluation, cancellationToken);
        return evaluation;
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

    public async Task<AiEvaluationLog?> GetEvaluationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<AiEvaluationLog?> GetEvaluationByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetByMessageIdAsync(messageId, cancellationToken);
    }

    public async Task<bool> ExistsForMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.ExistsForMessageAsync(messageId, cancellationToken);
    }

    public async Task<(IReadOnlyList<AiEvaluationLog> Items, int TotalCount)> GetPagedEvaluationsAsync(
        int pageNumber,
        int pageSize,
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        string? evaluatorRole = null,
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            isHelpful,
            rating,
            evaluatorUserId,
            evaluatorRole,
            cancellationToken);
    }

    public async Task<AiEvaluationSummary> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return await evaluationRepository.GetSummaryAsync(cancellationToken);
    }
}

