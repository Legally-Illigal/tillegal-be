using Microsoft.AspNetCore.Identity;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Infrastructure.SystemServices;

public sealed class PasswordHashingService : IPasswordHashingService
{
    private static readonly object PasswordSubject = new();
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _passwordHasher.HashPassword(PasswordSubject, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(providedPassword);

        var result = _passwordHasher.VerifyHashedPassword(PasswordSubject, hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
