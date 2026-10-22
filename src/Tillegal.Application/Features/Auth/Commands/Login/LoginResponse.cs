namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UserDto User);

public sealed record UserDto(
    Guid Id,
    string FullName,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? Role);
