namespace Tillegal.Application.Features.Auth.Models;

public sealed record PendingRegistration(
    string Email,
    string PasswordHash,
    string FullName,
    string? PhoneNumber,
    string OtpCode,
    DateTimeOffset CreatedAt);
