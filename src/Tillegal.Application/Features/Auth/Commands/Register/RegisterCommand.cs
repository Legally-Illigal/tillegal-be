using MediatR;

namespace Tillegal.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber = null) : IRequest<RegisterResult>;

public sealed record RegisterResult(string Message, int ExpirationMinutes);
