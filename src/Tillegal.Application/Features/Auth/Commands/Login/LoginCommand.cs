using MediatR;
using Tillegal.Application.Features.Auth.DTOs;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string EmailOrUserName,
    string Password,
    string? IpAddress = null) : IRequest<AuthDto>;

