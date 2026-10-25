using MediatR;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    LoginUserDto User);

public sealed record LoginUserDto(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? Role);
