using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Domain.Services;

public sealed class AuthService(IUserRepository userRepository)
{
    public async Task ValidateEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var exists = await userRepository.ExistsByEmailAsync(email, cancellationToken);
        if (exists)
        {
            throw new DomainException($"Email '{email}' is already registered.");
        }
    }

    public async Task<User> CreateUserAsync(
        string email,
        string passwordHash,
        string fullName,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        await ValidateEmailAvailabilityAsync(email, cancellationToken);

        var defaultRole = await userRepository.GetDefaultRoleAsync("User", cancellationToken)
            ?? throw new DomainException("Default user role 'User' could not be found.");

        var user = new User
        {
            RoleId = defaultRole.Id,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phoneNumber
        };

        await userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    public async Task<User?> GetUserForLoginAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return await userRepository.GetByEmailAsync(email, cancellationToken);
    }
}
