namespace Tillegal.Api.Contracts.Responses;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? Role);
