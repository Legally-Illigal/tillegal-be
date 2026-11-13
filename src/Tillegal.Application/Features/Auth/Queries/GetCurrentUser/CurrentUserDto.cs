namespace Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record CurrentUserDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Role,
    bool IsActive);

