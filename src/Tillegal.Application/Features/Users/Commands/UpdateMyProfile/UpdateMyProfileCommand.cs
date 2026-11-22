using MediatR;
using Tillegal.Application.Features.Users.Models;

namespace Tillegal.Application.Features.Users.Commands.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(
    string FullName,
    string? PhoneNumber) : IRequest<UserProfileDto>;
