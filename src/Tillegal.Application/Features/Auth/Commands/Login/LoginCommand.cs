using MediatR;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string EmailOrUserName,
    string Password,
    string? IpAddress = null) : IRequest<LoginResponse>;
