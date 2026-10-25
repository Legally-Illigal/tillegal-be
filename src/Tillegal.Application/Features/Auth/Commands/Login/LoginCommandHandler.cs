using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Mappings;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IAuthService authService,
    IPasswordHashingService passwordHashingService,
    IJwtTokenGenerator jwtTokenGenerator,
    IMapper mapper) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await authService.GetUserForLoginAsync(normalizedEmail, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = passwordHashingService.VerifyPassword(user.PasswordHash, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var roleName = user.Role?.Name ?? "User";
        var tokenResult = jwtTokenGenerator.GenerateToken(user, roleName);

        return mapper.Map<LoginResult>(new LoginResultMappingSource(user, tokenResult));
    }
}
