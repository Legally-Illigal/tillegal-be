using Tillegal.Domain.Common;

namespace Tillegal.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    public Guid ConversationId { get; set; }

    public required string Role { get; set; }

    public required string Content { get; set; }
}
