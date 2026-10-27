using Tillegal.Domain.Common;

namespace Tillegal.Domain.Entities;

public sealed class Conversation : BaseEntity
{
    public Guid UserId { get; set; }

    public required string Title { get; set; }
}
