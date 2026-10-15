using Microsoft.AspNetCore.Identity;

using Tillegal.Application.Authentication;

namespace Tillegal.Infrastructure.Persistence.PostgreSQL;

public sealed class UserAccountService(
    UserManager<ApplicationUser> userManager) : IUserAccountService
{
    public async Task<UserRegistrationResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        IdentityResult result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return new UserRegistrationResult(
                null,
                result.Errors.Select(error => error.Description).ToArray());
        }

        return new UserRegistrationResult(
            new UserAccount(user.Id.ToString(), user.Email!, Array.Empty<string>()),
            Array.Empty<string>());
    }

    public async Task<UserAccount?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return null;
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        IList<string> roles = await userManager.GetRolesAsync(user);
        return new UserAccount(user.Id.ToString(), user.Email!, roles.ToArray());
    }

    public async Task<UserAccount?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(userId, out Guid id))
        {
            return null;
        }

        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());
        if (user?.Email is null)
        {
            return null;
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        return new UserAccount(user.Id.ToString(), user.Email, roles.ToArray());
    }
}
