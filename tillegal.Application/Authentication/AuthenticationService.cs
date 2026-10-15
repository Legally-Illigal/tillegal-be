namespace Tillegal.Application.Authentication;

public sealed class AuthenticationService(
    IUserAccountService userAccountService,
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenStore refreshTokenStore) : IAuthenticationService
{
    public Task<UserRegistrationResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken) =>
        userAccountService.RegisterAsync(email, password, cancellationToken);

    public async Task<AuthSession?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        UserAccount? user = await userAccountService.ValidateCredentialsAsync(
            email,
            password,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthSession?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        RefreshTokenSession? refreshSession = await refreshTokenStore.RotateAsync(
            refreshToken,
            cancellationToken);
        if (refreshSession is null)
        {
            return null;
        }

        UserAccount? user = await userAccountService.FindByIdAsync(
            refreshSession.UserId,
            cancellationToken);
        if (user is null)
        {
            await refreshTokenStore.RevokeAsync(
                refreshSession.Token,
                cancellationToken);
            return null;
        }

        AccessTokenResult accessToken = accessTokenIssuer.Issue(user);
        return new AuthSession(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshSession.Token,
            refreshSession.ExpiresAt);
    }

    public Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken) =>
        refreshTokenStore.RevokeAsync(refreshToken, cancellationToken);

    private async Task<AuthSession> CreateSessionAsync(
        UserAccount user,
        CancellationToken cancellationToken)
    {
        AccessTokenResult accessToken = accessTokenIssuer.Issue(user);
        RefreshTokenSession refreshToken = await refreshTokenStore.IssueAsync(
            user.Id,
            cancellationToken);

        return new AuthSession(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }
}
