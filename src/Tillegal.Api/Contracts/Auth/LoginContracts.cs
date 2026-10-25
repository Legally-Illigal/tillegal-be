namespace Tillegal.Api.Contracts.Auth;

public sealed record LoginApiRequest(string Email, string Password);

public sealed record LoginApiResponse(
    LoginUserApiResponse User,
    string Message = "Login successful.");

public sealed record LoginUserApiResponse(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? Role);
