using MediatR;
using Tillegal.Application.Features.Auth.Commands.Login;

namespace Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;
