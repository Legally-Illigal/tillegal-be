namespace Tillegal.Application.Authentication;

public interface IRefreshTokenStore
{
    Task<RefreshTokenSession> IssueAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<RefreshTokenSession?> RotateAsync(
        string token,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        string token,
        CancellationToken cancellationToken);
}
