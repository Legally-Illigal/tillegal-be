using MediatR;

namespace Tillegal.Application.Features.Users.Commands.ChangeMyPassword;

public sealed record ChangeMyPasswordCommand(string NewPassword) : IRequest;
