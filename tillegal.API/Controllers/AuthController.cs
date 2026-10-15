using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Tillegal.API.Common;
using Tillegal.API.Models;
using Tillegal.Application.Authentication;

namespace Tillegal.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService,
    IOptions<RefreshTokenCookieOptions> cookieOptions) : ControllerBase
{
    private const string CsrfHeaderName = "X-CSRF-Token";
    private readonly RefreshTokenCookieOptions _cookieOptions = cookieOptions.Value;

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        UserRegistrationResult result = await authenticationService.RegisterAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse.Fail(
                HttpStatusCode.BadRequest,
                result.Errors.ToArray()));
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            result.User!.Id,
            result.User.Email,
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        AuthSession? result = await authenticationService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result is null)
        {
            return Unauthorized(ApiResponse.Fail(
                HttpStatusCode.Unauthorized,
                "Email hoặc mật khẩu không hợp lệ."));
        }

        var csrfToken = SetRefreshCookies(result);
        return Ok(new LoginResponse(
            result.AccessToken,
            result.AccessTokenExpiresAt,
            csrfToken));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!HasValidCsrfToken())
        {
            return Forbid();
        }

        if (!Request.Cookies.TryGetValue(_cookieOptions.Name, out var refreshToken))
        {
            return Unauthorized(ApiResponse.Fail(
                HttpStatusCode.Unauthorized,
                "Refresh token is missing or invalid."));
        }

        AuthSession? session = await authenticationService.RefreshAsync(
            refreshToken,
            cancellationToken);
        if (session is null)
        {
            ClearRefreshCookies();
            return Unauthorized(ApiResponse.Fail(
                HttpStatusCode.Unauthorized,
                "Refresh token is missing or invalid."));
        }

        var csrfToken = SetRefreshCookies(session);
        return Ok(new LoginResponse(
            session.AccessToken,
            session.AccessTokenExpiresAt,
            csrfToken));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!HasValidCsrfToken())
        {
            return Forbid();
        }

        if (Request.Cookies.TryGetValue(_cookieOptions.Name, out var refreshToken))
        {
            await authenticationService.RevokeRefreshTokenAsync(
                refreshToken,
                cancellationToken);
        }

        ClearRefreshCookies();
        return NoContent();
    }

    private string SetRefreshCookies(AuthSession session)
    {
        var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        SameSiteMode sameSite = Enum.Parse<SameSiteMode>(_cookieOptions.SameSite, true);
        TimeSpan maxAge = session.RefreshTokenExpiresAt - DateTimeOffset.UtcNow;

        Response.Cookies.Append(_cookieOptions.Name, session.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Path = "/api/auth",
            Domain = _cookieOptions.Domain,
            IsEssential = true,
            MaxAge = maxAge,
        });

        Response.Cookies.Append(_cookieOptions.CsrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = sameSite,
            Path = "/api/auth",
            Domain = _cookieOptions.Domain,
            IsEssential = true,
            MaxAge = maxAge,
        });

        return csrfToken;
    }

    private bool HasValidCsrfToken()
    {
        if (!Request.Cookies.TryGetValue(_cookieOptions.CsrfCookieName, out var cookieToken)
            || !Request.Headers.TryGetValue(CsrfHeaderName, out StringValues headerValues))
        {
            return false;
        }

        var headerToken = headerValues.ToString();
        if (string.IsNullOrEmpty(headerToken))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(cookieToken),
            Encoding.UTF8.GetBytes(headerToken));
    }

    private void ClearRefreshCookies()
    {
        SameSiteMode sameSite = Enum.Parse<SameSiteMode>(_cookieOptions.SameSite, true);
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Path = "/api/auth",
            Domain = _cookieOptions.Domain,
        };
        Response.Cookies.Delete(_cookieOptions.Name, options);
        options.HttpOnly = false;
        Response.Cookies.Delete(_cookieOptions.CsrfCookieName, options);
    }
}

public sealed record RegisterRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(12)] string Password);

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string? CsrfToken);
