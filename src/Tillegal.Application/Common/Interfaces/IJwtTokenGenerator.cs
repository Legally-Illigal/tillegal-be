using Tillegal.Domain.Entities;

namespace Tillegal.Application.Common.Interfaces;

public sealed record JwtTokenResult(string Token, int ExpiresInSeconds);

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(User user, string? roleName = null);
}
