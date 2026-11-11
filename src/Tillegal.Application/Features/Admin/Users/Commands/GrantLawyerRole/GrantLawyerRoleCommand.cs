using MediatR;
using Tillegal.Application.Features.Admin.Users.Models;

namespace Tillegal.Application.Features.Admin.Users.Commands.GrantLawyerRole;

public sealed record GrantLawyerRoleCommand(Guid UserId) : IRequest<UserDto>;

