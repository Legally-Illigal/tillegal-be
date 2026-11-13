namespace Tillegal.Api.Contracts.Auth;

public sealed record CurrentUserApiResponse(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Role,
    bool IsActive);

