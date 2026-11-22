using MediatR;
using Tillegal.Application.Features.Users.Models;

namespace Tillegal.Application.Features.Users.Queries.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<UserProfileDto>;
