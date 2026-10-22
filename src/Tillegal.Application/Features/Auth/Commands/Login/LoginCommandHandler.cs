using System.Text.Json;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordHashingService passwordHashingService,
    IJwtTokenGenerator jwtTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByEmailOrUserNameAsync(request.EmailOrUserName, cancellationToken);
        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordHashingService.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException("Invalid email/username or password.");
        }

        var ip = string.IsNullOrWhiteSpace(request.IpAddress) ? "127.0.0.1" : request.IpAddress;

        var auditLog = new AuditLog
        {
            EventType = "AUTH_EVENT",
            ActorUserId = user.Id,
            ActorRole = user.Role?.Name ?? "User",
            IpAddress = ip,
            TargetType = "User",
            TargetUserId = user.Id,
            Action = "LOGIN_SUCCESS",
            Status = "SUCCESS",
            Description = $"Người dùng đăng nhập thành công vào hệ thống từ địa chỉ IP {ip}.",
            NewValues = JsonDocument.Parse($"{{\"method\":\"Password\",\"ipAddress\":\"{ip}\"}}")
        };

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await userRepository.UpdateLastActiveAtAsync(user.Id, dateTimeProvider.UtcNow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tokenResult = jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(
            tokenResult.AccessToken,
            "Bearer",
            tokenResult.ExpiresIn,
            new UserDto(
                user.Id,
                user.FullName,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Role?.Name));
    }
}
