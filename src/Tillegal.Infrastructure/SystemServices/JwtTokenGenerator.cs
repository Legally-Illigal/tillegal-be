using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Domain.Entities;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.SystemServices;

public sealed class JwtTokenGenerator(
    IOptions<JwtOptions> options,
    IDateTimeProvider dateTimeProvider) : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public JwtTokenResult GenerateToken(User user, string? roleName = null)
    {
        var generatedJwt = CreateToken(user, roleName);
        var expirationMinutes = _jwtOptions.ExpirationMinutes > 0 ? _jwtOptions.ExpirationMinutes : 60;
        return new JwtTokenResult(generatedJwt.Token, expirationMinutes * 60);
    }

    public GeneratedJwt Generate(Guid userId, string email, string fullName)
    {
        var user = new User
        {
            Id = userId,
            Email = email,
            FullName = fullName
        };
        var generatedJwt = CreateToken(user);
        return new GeneratedJwt(generatedJwt.Token, generatedJwt.ExpiresAt);
    }

    private (string Token, DateTimeOffset ExpiresAt) CreateToken(User user, string? roleName = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var resolvedRole = roleName ?? user.Role?.Name ?? "User";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, resolvedRole),
            new("role", resolvedRole),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        var expirationMinutes = _jwtOptions.ExpirationMinutes > 0 ? _jwtOptions.ExpirationMinutes : 60;
        var issuedAt = dateTimeProvider.UtcNow;
        var expiresAt = issuedAt.AddMinutes(expirationMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = signingCredentials,
            Issuer = string.IsNullOrWhiteSpace(_jwtOptions.Issuer) ? null : _jwtOptions.Issuer,
            Audience = string.IsNullOrWhiteSpace(_jwtOptions.Audience) ? null : _jwtOptions.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
