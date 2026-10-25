using MediatR;

namespace Tillegal.Application.Features.Auth.Commands.RegisterWithGoogle;

public sealed record RegisterWithGoogleCommand(string IdToken) : IRequest<GoogleRegistrationResult>;

public sealed record GoogleRegistrationResult(
    Guid UserId,
    string Email,
    string FullName,
    string Message,
    string AccessToken,
    DateTimeOffset ExpiresAt);
