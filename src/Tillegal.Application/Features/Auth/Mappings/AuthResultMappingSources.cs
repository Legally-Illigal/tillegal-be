using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Auth.Mappings;

public sealed record LoginResultMappingSource(User User, JwtTokenResult Token);

public sealed record GoogleAuthResultMappingSource(User User, GeneratedJwt Token, string Message);
