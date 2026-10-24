using System.Text.Json;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Services;

namespace Tillegal.Application.Features.Auth.Commands.VerifyRegistration;

public sealed class VerifyRegistrationCommandHandler(
    AuthService authService,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<VerifyRegistrationCommand, VerifyRegistrationResult>
{
    public async Task<VerifyRegistrationResult> Handle(
        VerifyRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cacheKey = $"pending_reg:{normalizedEmail}";

        var cachedBytes = await cacheService.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is null || cachedBytes.Length == 0)
        {
            throw new DomainException("Verification code has expired or registration request does not exist. Please register again.");
        }

        var pendingRegistration = JsonSerializer.Deserialize<PendingRegistration>(cachedBytes);
        if (pendingRegistration is null)
        {
            throw new DomainException("Invalid registration data. Please register again.");
        }

        if (!string.Equals(pendingRegistration.OtpCode, request.Otp.Trim(), StringComparison.Ordinal))
        {
            throw new DomainException("Invalid verification code.");
        }

        // Create user via Domain Service
        var createdUser = await authService.CreateUserAsync(
            email: pendingRegistration.Email,
            passwordHash: pendingRegistration.PasswordHash,
            fullName: pendingRegistration.FullName,
            phoneNumber: pendingRegistration.PhoneNumber,
            cancellationToken: cancellationToken);

        // Commit transaction / save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Remove from cache
        await cacheService.RemoveAsync(cacheKey, cancellationToken);

        return new VerifyRegistrationResult(
            UserId: createdUser.Id,
            Email: createdUser.Email ?? pendingRegistration.Email,
            FullName: createdUser.FullName,
            Message: "Account registration and verification completed successfully.");
    }
}
