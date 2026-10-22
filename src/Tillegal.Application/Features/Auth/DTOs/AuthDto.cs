namespace Tillegal.Application.Features.Auth.DTOs;

public sealed record AuthDto(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UserDto User);
