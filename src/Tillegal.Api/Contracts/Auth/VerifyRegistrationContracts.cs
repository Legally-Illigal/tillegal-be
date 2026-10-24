namespace Tillegal.Api.Contracts.Auth;

public sealed record VerifyRegistrationApiRequest(string Email, string Otp);

public sealed record VerifyRegistrationApiResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Message);

public sealed record ResendOtpApiRequest(string Email);

public sealed record ResendOtpApiResponse(
    string Message,
    int ExpirationMinutes);
