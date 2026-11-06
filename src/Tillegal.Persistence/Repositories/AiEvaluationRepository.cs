using Microsoft.EntityFrameworkCore;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Models;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Repositories;

public sealed class AiEvaluationRepository(AppDbContext dbContext) : IAiEvaluationRepository
{
    public async Task<AiEvaluationLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var model = await dbContext.AiEvaluationLogs
            .AsNoTracking()
            .Include(e => e.EvaluatorUser)
                .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    public async Task<AiEvaluationLog?> GetByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var model = await dbContext.AiEvaluationLogs
            .AsNoTracking()
            .Include(e => e.EvaluatorUser)
                .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(e => e.MessageId == messageId, cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    public async Task<bool> ExistsForMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AiEvaluationLogs
            .AnyAsync(e => e.MessageId == messageId, cancellationToken);
    }

    public async Task AddAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var model = ToDataModel(evaluation);
        await dbContext.AiEvaluationLogs.AddAsync(model, cancellationToken);
    }

    public async Task UpdateAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var model = await dbContext.AiEvaluationLogs
            .FirstOrDefaultAsync(e => e.Id == evaluation.Id, cancellationToken);

        if (model is null)
        {
            model = ToDataModel(evaluation);
            dbContext.AiEvaluationLogs.Update(model);
            return;
        }

        model.IsHelpful = evaluation.IsHelpful;
        model.Rating = evaluation.Rating;
        model.FeedbackComment = evaluation.FeedbackComment;
        model.UpdatedAt = evaluation.UpdatedAt ?? DateTimeOffset.UtcNow;
    }

    public async Task DeleteAsync(
        AiEvaluationLog evaluation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var model = await dbContext.AiEvaluationLogs
            .FirstOrDefaultAsync(e => e.Id == evaluation.Id, cancellationToken);

        if (model is not null)
        {
            model.DeletedAt = evaluation.DeletedAt ?? DateTimeOffset.UtcNow;
            model.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public async Task<(IReadOnlyList<AiEvaluationLog> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isHelpful = null,
        int? rating = null,
        Guid? evaluatorUserId = null,
        string? evaluatorRole = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AiEvaluationLogs
            .AsNoTracking()
            .Include(e => e.EvaluatorUser)
                .ThenInclude(u => u!.Role)
            .AsQueryable();

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

        if (!string.IsNullOrWhiteSpace(evaluatorRole))
        {
            var roleFilter = evaluatorRole.Trim();
            if (string.Equals(roleFilter, "Lawyer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleFilter, "Lawer", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.EvaluatorUser != null &&
                    (e.EvaluatorUser.Role.Name == "Lawer" || e.EvaluatorUser.Role.Name == "Lawyer"));
            }
            else
            {
                query = query.Where(e => e.EvaluatorUser != null &&
                    e.EvaluatorUser.Role.Name == roleFilter);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = models.Select(ToDomain).ToList();
        return (items, totalCount);
    }

    public async Task<AiEvaluationSummary> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AiEvaluationLogs
            .AsNoTracking()
            .Include(e => e.EvaluatorUser)
                .ThenInclude(u => u!.Role)
            .AsQueryable();

        var totalEvaluations = await query.CountAsync(cancellationToken);
        var helpfulCount = await query.CountAsync(e => e.IsHelpful == true, cancellationToken);
        var unhelpfulCount = await query.CountAsync(e => e.IsHelpful == false, cancellationToken);

        var ratedEvaluations = query.Where(e => e.Rating.HasValue);
        var averageRating = await ratedEvaluations.AnyAsync(cancellationToken)
            ? await ratedEvaluations.AverageAsync(e => (double)e.Rating!.Value, cancellationToken)
            : 0.0;

        var lawyerCount = await query.CountAsync(e => e.EvaluatorUser != null &&
            (e.EvaluatorUser.Role.Name == "Lawer" || e.EvaluatorUser.Role.Name == "Lawyer"),
            cancellationToken);

        var userCount = await query.CountAsync(e => e.EvaluatorUser != null &&
            e.EvaluatorUser.Role.Name == "User",
            cancellationToken);

        var ratingDistribution = new Dictionary<int, int>();
        for (var star = 1; star <= 5; star++)
        {
            var starCount = await query.CountAsync(e => e.Rating == star, cancellationToken);
            ratingDistribution[star] = starCount;
        }

        return new AiEvaluationSummary(
            totalEvaluations,
            helpfulCount,
            unhelpfulCount,
            Math.Round(averageRating, 2),
            lawyerCount,
            userCount,
            ratingDistribution);
    }

    private static AiEvaluationLog ToDomain(AiEvaluationLogDataModel model)
    {
        var role = model.EvaluatorUser?.Role?.Name;
        // Normalize "Lawer" to "Lawyer" for consistent presentation
        if (string.Equals(role, "Lawer", StringComparison.OrdinalIgnoreCase))
        {
            role = "Lawyer";
        }

        return new AiEvaluationLog(
            model.Id,
            model.MessageId,
            model.EvaluatorUserId,
            model.IsHelpful,
            model.Rating,
            model.FeedbackComment,
            model.CreatedAt,
            model.UpdatedAt,
            model.DeletedAt,
            role);
    }

    private static AiEvaluationLogDataModel ToDataModel(AiEvaluationLog domain)
    {
        return new AiEvaluationLogDataModel
        {
            Id = domain.Id,
            MessageId = domain.MessageId,
            EvaluatorUserId = domain.EvaluatorUserId,
            IsHelpful = domain.IsHelpful,
            Rating = domain.Rating,
            FeedbackComment = domain.FeedbackComment,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,
            DeletedAt = domain.DeletedAt
        };
    }
}

