using Tillegal.Application.Common.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    TokenResult GenerateToken(User user);
}
