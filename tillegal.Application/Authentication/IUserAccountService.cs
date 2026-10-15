namespace Tillegal.Application.Authentication;

public interface IUserAccountService
{
    Task<UserRegistrationResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<UserAccount?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<UserAccount?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken);
}
