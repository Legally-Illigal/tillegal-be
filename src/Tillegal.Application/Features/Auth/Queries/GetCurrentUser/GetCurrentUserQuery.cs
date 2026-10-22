using MediatR;
using Tillegal.Application.Features.Auth.DTOs;

namespace Tillegal.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;
