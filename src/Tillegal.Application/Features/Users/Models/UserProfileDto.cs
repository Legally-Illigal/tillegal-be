namespace Tillegal.Application.Features.Users.Models;

public sealed record UserProfileDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActiveAt);
