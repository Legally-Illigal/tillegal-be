namespace Tillegal.Api.Contracts.Responses;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UserResponse User);
