using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Services;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    AuthService authService,
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
        var userDto = mapper.Map<LoginUserDto>(user);

        return new LoginResult(
            AccessToken: tokenResult.Token,
            TokenType: "Bearer",
            ExpiresIn: tokenResult.ExpiresInSeconds,
            User: userDto);
    }
}
