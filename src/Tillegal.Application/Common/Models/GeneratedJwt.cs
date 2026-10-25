namespace Tillegal.Application.Common.Models;

public sealed record GeneratedJwt(string AccessToken, DateTimeOffset ExpiresAt);
