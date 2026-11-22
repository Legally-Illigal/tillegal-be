namespace Tillegal.Api.Contracts.Users;

public sealed record UpdateUserProfileApiRequest(
    string FullName,
    string? PhoneNumber);

public sealed record ChangePasswordApiRequest(
    string NewPassword);

public sealed record UserProfileApiResponse(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActiveAt);

public sealed record ChangePasswordApiResponse(string Message = "Password has been successfully updated.");
