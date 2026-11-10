namespace Tillegal.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

public sealed class UserAttachmentRepository(AppDbContext dbContext) : IUserAttachmentRepository
{
    public async Task<UserAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataModel = await dbContext.UserAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<IReadOnlyList<UserAttachment>> GetByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var dataModels = await dbContext.UserAttachments
            .AsNoTracking()
            .Where(a => a.ConversationId == conversationId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return dataModels.Select(MapToEntity).ToList();
    }

    public async Task<IReadOnlyList<UserAttachment>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dataModels = await dbContext.UserAttachments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return dataModels.Select(MapToEntity).ToList();
    }

    public async Task AddAsync(UserAttachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var dataModel = MapToDataModel(attachment);
        await dbContext.UserAttachments.AddAsync(dataModel, cancellationToken);
    }

    public async Task UpdateAsync(UserAttachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var dataModel = await dbContext.UserAttachments
            .FirstOrDefaultAsync(a => a.Id == attachment.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.ProcessingStatus = attachment.ProcessingStatus;
            dataModel.TokenCount = attachment.TokenCount;
            dataModel.UpdatedAt = attachment.UpdatedAt;
            dataModel.DeletedAt = attachment.DeletedAt;
        }
    }

    public async Task DeleteAsync(UserAttachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var dataModel = await dbContext.UserAttachments
            .FirstOrDefaultAsync(a => a.Id == attachment.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.DeletedAt = attachment.DeletedAt ?? DateTimeOffset.UtcNow;
            dataModel.UpdatedAt = attachment.UpdatedAt ?? DateTimeOffset.UtcNow;
        }
    }

    public async Task HardDeleteAsync(UserAttachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var dataModel = await dbContext.UserAttachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == attachment.Id, cancellationToken);

        if (dataModel is not null)
        {
            dbContext.UserAttachments.Remove(dataModel);
        }
    }

    private static UserAttachment MapToEntity(UserAttachmentDataModel dataModel)
    {
        return new UserAttachment(
            dataModel.Id,
            dataModel.ConversationId,
            dataModel.UserId,
            dataModel.FileName,
            dataModel.StoragePath,
            dataModel.FileSize,
            dataModel.ContentType,
            dataModel.ProcessingStatus,
            dataModel.TokenCount,
            dataModel.CreatedAt,
            dataModel.UpdatedAt,
            dataModel.DeletedAt,
            dataModel.FileHash);
    }

    private static UserAttachmentDataModel MapToDataModel(UserAttachment entity)
    {
        return new UserAttachmentDataModel
        {
            Id = entity.Id,
            ConversationId = entity.ConversationId,
            UserId = entity.UserId,
            FileName = entity.FileName,
            StoragePath = entity.StoragePath,
            FileSize = entity.FileSize,
            ContentType = entity.ContentType,
            ProcessingStatus = entity.ProcessingStatus,
            TokenCount = entity.TokenCount,
            FileHash = entity.FileHash,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }
}

