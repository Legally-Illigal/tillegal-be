using MediatR;
using Tillegal.Application.Features.Admin.Users.Models;

namespace Tillegal.Application.Features.Admin.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null) : IRequest<PagedUsersDto>;
