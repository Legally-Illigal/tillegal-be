namespace Tillegal.Application.Features.Admin.Users.Models;

public sealed record PagedUsersDto(
    IReadOnlyList<UserDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
