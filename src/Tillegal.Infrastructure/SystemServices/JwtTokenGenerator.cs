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
    IOptions<JwtOptions> jwtOptions,
    IDateTimeProvider dateTimeProvider) : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public TokenResult GenerateToken(User user)
    {
        var signingKey = !string.IsNullOrWhiteSpace(_jwtOptions.SigningKey)
            ? _jwtOptions.SigningKey
            : "TillegalDefaultDevelopmentSecretKeyMustBe32BytesLong!";

        var keyBytes = Encoding.UTF8.GetBytes(signingKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            claims.Add(new Claim("username", user.UserName));
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));
        }

        if (!string.IsNullOrWhiteSpace(user.Role?.Name))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var expirationMinutes = _jwtOptions.ExpirationMinutes > 0 ? _jwtOptions.ExpirationMinutes : 60;
        var expires = dateTimeProvider.UtcNow.AddMinutes(expirationMinutes).UtcDateTime;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = string.IsNullOrWhiteSpace(_jwtOptions.Issuer) ? null : _jwtOptions.Issuer,
            Audience = string.IsNullOrWhiteSpace(_jwtOptions.Audience) ? null : _jwtOptions.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new TokenResult(tokenString, expirationMinutes * 60);
    }
}
