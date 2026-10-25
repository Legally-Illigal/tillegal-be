using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Services;

public interface IAuthService
{
    Task ValidateEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default);

    Task<User> CreateUserAsync(
        string email,
        string? passwordHash,
        string fullName,
        string? phoneNumber = null,
        string? googleSubject = null,
        CancellationToken cancellationToken = default);

    Task<User?> GetUserForLoginAsync(string email, CancellationToken cancellationToken = default);
}
