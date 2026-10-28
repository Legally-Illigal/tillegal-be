namespace Tillegal.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

public sealed class ConversationRepository(AppDbContext dbContext) : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataModel = await dbContext.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<IReadOnlyList<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var dataModels = await dbContext.Conversations
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        return dataModels.Select(MapToEntity).ToList();
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        var dataModel = new ConversationDataModel
        {
            Id = conversation.Id,
            UserId = conversation.UserId,
            Title = conversation.Title,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            DeletedAt = conversation.DeletedAt
        };

        await dbContext.Conversations.AddAsync(dataModel, cancellationToken);
    }

    public async Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        var dataModel = await dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversation.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.Title = conversation.Title;
            dataModel.UpdatedAt = conversation.UpdatedAt;
            dataModel.DeletedAt = conversation.DeletedAt;
        }
    }

    public async Task DeleteAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        var dataModel = await dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversation.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.DeletedAt = conversation.DeletedAt ?? DateTimeOffset.UtcNow;
            dataModel.UpdatedAt = conversation.UpdatedAt ?? DateTimeOffset.UtcNow;
        }
    }

    private static Conversation MapToEntity(ConversationDataModel dataModel)
    {
        return new Conversation(
            dataModel.Id,
            dataModel.UserId,
            dataModel.Title,
            dataModel.CreatedAt,
            dataModel.UpdatedAt,
            dataModel.DeletedAt);
    }
}
