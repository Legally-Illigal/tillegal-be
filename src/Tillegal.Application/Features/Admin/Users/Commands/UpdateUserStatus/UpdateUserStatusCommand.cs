using MediatR;
using Tillegal.Application.Features.Admin.Users.Models;

namespace Tillegal.Application.Features.Admin.Users.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(Guid Id, bool IsActive) : IRequest<UserDto>;

