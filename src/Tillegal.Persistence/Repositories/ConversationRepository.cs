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
