namespace Tillegal.Api.Contracts.Admin;

public sealed record UpdateUserStatusApiRequest(
    bool IsActive);

public sealed record AdminUserApiResponse(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? RoleName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActiveAt);

public sealed record PagedAdminUsersApiResponse(
    IReadOnlyList<AdminUserApiResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
