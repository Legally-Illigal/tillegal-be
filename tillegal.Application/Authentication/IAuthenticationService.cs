namespace Tillegal.Application.Authentication;

public interface IAuthenticationService
{
    Task<UserRegistrationResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthSession?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthSession?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);
}
