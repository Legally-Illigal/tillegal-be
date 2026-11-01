using Microsoft.EntityFrameworkCore;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Repositories;

public sealed class AiEvaluationRepository(AppDbContext dbContext) : IAiEvaluationRepository
{
    public async Task<AiEvaluationLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dataModel = await dbContext.AiEvaluationLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<AiEvaluationLog?> GetByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var dataModel = await dbContext.AiEvaluationLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.MessageId == messageId, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<IReadOnlyList<AiEvaluationLog>> GetByEvaluatorUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dataModels = await dbContext.AiEvaluationLogs
            .AsNoTracking()
            .Where(e => e.EvaluatorUserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

        return dataModels.Select(MapToEntity).ToList();
    }

    public async Task<IReadOnlyList<AiEvaluationLog>> GetPagedAsync(
        int skip,
        int take,
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(dbContext.AiEvaluationLogs.AsNoTracking(), isHelpful, rating, evaluatorUserId);

        var dataModels = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return dataModels.Select(MapToEntity).ToList();
    }

    public async Task<int> GetCountAsync(
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(dbContext.AiEvaluationLogs.AsNoTracking(), isHelpful, rating, evaluatorUserId);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<(int TotalCount, double AverageRating, int HelpfulCount, int UnhelpfulCount)> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AiEvaluationLogs.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return (0, 0.0, 0, 0);
        }

        var ratedQuery = query.Where(e => e.Rating.HasValue);
        var ratedCount = await ratedQuery.CountAsync(cancellationToken);
        var averageRating = ratedCount > 0
            ? await ratedQuery.AverageAsync(e => (double)e.Rating!.Value, cancellationToken)
            : 0.0;

        var helpfulCount = await query.CountAsync(e => e.IsHelpful == true, cancellationToken);
        var unhelpfulCount = await query.CountAsync(e => e.IsHelpful == false, cancellationToken);

        return (totalCount, Math.Round(averageRating, 2), helpfulCount, unhelpfulCount);
    }

    public async Task AddAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var dataModel = MapToDataModel(evaluation);
        await dbContext.AiEvaluationLogs.AddAsync(dataModel, cancellationToken);
    }

    public async Task UpdateAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var dataModel = await dbContext.AiEvaluationLogs
            .FirstOrDefaultAsync(e => e.Id == evaluation.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.IsHelpful = evaluation.IsHelpful;
            dataModel.Rating = evaluation.Rating;
            dataModel.FeedbackComment = evaluation.FeedbackComment;
            dataModel.UpdatedAt = evaluation.UpdatedAt;
            dataModel.DeletedAt = evaluation.DeletedAt;
        }
    }

    public async Task DeleteAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var dataModel = await dbContext.AiEvaluationLogs
            .FirstOrDefaultAsync(e => e.Id == evaluation.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.DeletedAt = evaluation.DeletedAt ?? DateTimeOffset.UtcNow;
            dataModel.UpdatedAt = evaluation.UpdatedAt ?? DateTimeOffset.UtcNow;
        }
    }

    private static IQueryable<AiEvaluationLogDataModel> ApplyFilters(
        IQueryable<AiEvaluationLogDataModel> query,
        bool? isHelpful,
        int? rating,
        Guid? evaluatorUserId)
    {
        if (isHelpful.HasValue)
        {
            query = query.Where(e => e.IsHelpful == isHelpful.Value);
        }

        if (rating.HasValue)
        {
            query = query.Where(e => e.Rating == rating.Value);
        }

        if (evaluatorUserId.HasValue)
        {
            query = query.Where(e => e.EvaluatorUserId == evaluatorUserId.Value);
        }

        return query;
    }

    private static AiEvaluationLog MapToEntity(AiEvaluationLogDataModel dataModel)
    {
        return new AiEvaluationLog(
            dataModel.Id,
            dataModel.MessageId,
            dataModel.EvaluatorUserId,
            dataModel.IsHelpful,
            dataModel.Rating,
            dataModel.FeedbackComment,
            dataModel.CreatedAt,
            dataModel.UpdatedAt,
            dataModel.DeletedAt);
    }

    private static AiEvaluationLogDataModel MapToDataModel(AiEvaluationLog entity)
    {
        return new AiEvaluationLogDataModel
        {
            Id = entity.Id,
            MessageId = entity.MessageId,
            EvaluatorUserId = entity.EvaluatorUserId,
            IsHelpful = entity.IsHelpful,
            Rating = entity.Rating,
            FeedbackComment = entity.FeedbackComment,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }
}

