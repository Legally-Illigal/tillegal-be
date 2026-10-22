namespace Tillegal.Application.Features.Auth.DTOs;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? Role);
