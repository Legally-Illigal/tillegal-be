namespace Tillegal.Application.Authentication;

public sealed record UserAccount(
    string Id,
    string Email,
    IReadOnlyCollection<string> Roles);

public sealed record UserRegistrationResult(
    UserAccount? User,
    IReadOnlyCollection<string> Errors)
{
    public bool Succeeded => User is not null;
}

public sealed record AccessTokenResult(
    string Token,
    DateTimeOffset ExpiresAt);

public sealed record AuthSession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record RefreshTokenSession(
    string UserId,
    string Token,
    DateTimeOffset ExpiresAt);
