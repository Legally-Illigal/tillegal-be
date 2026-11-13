using MediatR;

namespace Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;

