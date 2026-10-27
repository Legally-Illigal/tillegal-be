using Tillegal.Domain.Entities;
using Tillegal.Application.Common.Models;

namespace Tillegal.Application.Common.Interfaces;

public sealed record JwtTokenResult(string Token, int ExpiresInSeconds);

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(User user, string? roleName = null);

    GeneratedJwt Generate(Guid userId, string email, string fullName);
}
