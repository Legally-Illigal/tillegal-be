namespace Tillegal.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public Guid RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastActiveAt { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }
}
