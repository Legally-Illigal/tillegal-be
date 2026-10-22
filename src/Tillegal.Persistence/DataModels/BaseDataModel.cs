namespace Tillegal.Persistence.DataModels;

public abstract class BaseDataModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
