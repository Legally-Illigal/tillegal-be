using MediatR;
using Tillegal.Application.Features.Admin.Users.Models;

namespace Tillegal.Application.Features.Admin.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
