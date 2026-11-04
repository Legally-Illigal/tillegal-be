namespace Tillegal.Application.Features.Admin.Users.Models;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? RoleName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActiveAt);
