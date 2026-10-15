namespace Tillegal.API.Common;

public sealed class RefreshTokenCookieOptions
{
    public string Name { get; init; } = "tillegal_refresh";
    public string CsrfCookieName { get; init; } = "tillegal_csrf";
    public string SameSite { get; init; } = "Lax";
    public string? Domain { get; init; }
}
