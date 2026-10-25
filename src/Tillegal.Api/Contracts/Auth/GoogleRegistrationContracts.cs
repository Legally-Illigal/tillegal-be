namespace Tillegal.Api.Contracts.Auth;

public sealed record GoogleRegistrationApiRequest(string IdToken);

public sealed record GoogleRegistrationApiResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Message);
