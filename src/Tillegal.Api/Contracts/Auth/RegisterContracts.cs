namespace Tillegal.Api.Contracts.Auth;

public sealed record RegisterApiRequest(
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber = null);


public sealed record RegisterApiResponse(
    string Message,
    int ExpirationMinutes);
