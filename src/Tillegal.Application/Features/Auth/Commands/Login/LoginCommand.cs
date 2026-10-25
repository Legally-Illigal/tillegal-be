using MediatR;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    User User);
