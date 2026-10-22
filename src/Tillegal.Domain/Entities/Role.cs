namespace Tillegal.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public string? Name { get; set; }
}
