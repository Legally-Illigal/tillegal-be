namespace Tillegal.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

public sealed class ConversationMessageRepository(AppDbContext dbContext) : IConversationMessageRepository
{
    public async Task<ConversationMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataModel = await dbContext.ConversationMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var dataModels = await dbContext.ConversationMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return dataModels.Select(MapToEntity).ToList();
    }

    public async Task AddAsync(ConversationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var dataModel = MapToDataModel(message);
        await dbContext.ConversationMessages.AddAsync(dataModel, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ConversationMessage> messages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var dataModels = messages.Select(MapToDataModel).ToList();
        await dbContext.ConversationMessages.AddRangeAsync(dataModels, cancellationToken);
    }

    public async Task UpdateAsync(ConversationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var dataModel = await dbContext.ConversationMessages
            .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.Content = message.Content;
            dataModel.Citations = message.Citations;
            dataModel.UpdatedAt = message.UpdatedAt;
            dataModel.DeletedAt = message.DeletedAt;
        }
    }

    public async Task DeleteAsync(ConversationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var dataModel = await dbContext.ConversationMessages
            .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

        if (dataModel is not null)
        {
            dataModel.DeletedAt = message.DeletedAt ?? DateTimeOffset.UtcNow;
            dataModel.UpdatedAt = message.UpdatedAt ?? DateTimeOffset.UtcNow;
        }
    }

    private static ConversationMessage MapToEntity(ConversationMessageDataModel dataModel)
    {
        return new ConversationMessage(
            dataModel.Id,
            dataModel.ConversationId,
            dataModel.Role,
            dataModel.Content,
            dataModel.Citations,
            dataModel.CreatedAt,
            dataModel.UpdatedAt,
            dataModel.DeletedAt);
    }

    private static ConversationMessageDataModel MapToDataModel(ConversationMessage entity)
    {
        return new ConversationMessageDataModel
        {
            Id = entity.Id,
            ConversationId = entity.ConversationId,
            Role = entity.Role,
            Content = entity.Content,
            Citations = entity.Citations,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }
}
