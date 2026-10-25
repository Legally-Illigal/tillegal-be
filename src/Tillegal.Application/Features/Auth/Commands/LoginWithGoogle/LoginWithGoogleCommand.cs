using MediatR;

namespace Tillegal.Application.Features.Auth.Commands.LoginWithGoogle;

public sealed record LoginWithGoogleCommand(string IdToken) : IRequest<GoogleLoginResult>;

public sealed record GoogleLoginResult(
    Guid UserId,
    string Email,
    string FullName,
    string Message,
    string AccessToken,
    DateTimeOffset ExpiresAt);
