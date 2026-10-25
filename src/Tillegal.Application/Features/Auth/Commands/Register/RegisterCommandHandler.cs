using System.Security.Cryptography;
using System.Text.Json;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IAuthService authService,
    IPasswordHashingService passwordHashingService,
    ICacheService cacheService,
    IEmailService emailService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<RegisterCommand, RegisterResult>
{
    private const int OtpExpirationMinutes = 10;

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if user already exists in DB
        await authService.ValidateEmailAvailabilityAsync(normalizedEmail, cancellationToken);

        // Generate 6-digit OTP
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Hash password
        var passwordHash = passwordHashingService.HashPassword(request.Password);

        // Prepare pending registration data
        var pendingRegistration = new PendingRegistration(
            Email: normalizedEmail,
            PasswordHash: passwordHash,
            FullName: request.FullName.Trim(),
            PhoneNumber: request.PhoneNumber?.Trim(),
            OtpCode: otpCode,
            CreatedAt: dateTimeProvider.UtcNow);

        var cacheKey = $"pending_reg:{normalizedEmail}";
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(pendingRegistration);

        // Save to cache with TTL
        await cacheService.SetAsync(
            cacheKey,
            payloadBytes,
            TimeSpan.FromMinutes(OtpExpirationMinutes),
            cancellationToken);

        // Send OTP email via Resend
        var emailSubject = "Mã xác thực đăng ký tài khoản Tillegal";
        var htmlBody = $"""
            <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;">
                <h2 style="color: #1e293b; text-align: center;">Xác thực tài khoản Tillegal</h2>
                <p>Xin chào <strong>{request.FullName.Trim()}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại Tillegal. Để hoàn tất đăng ký, vui lòng sử dụng mã xác thực (OTP) bên dưới:</p>
                <div style="margin: 24px 0; text-align: center;">
                    <span style="display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #2563eb; background-color: #eff6ff; padding: 12px 24px; border-radius: 6px; border: 1px dashed #93c5fd;">
                        {otpCode}
                    </span>
                </div>
                <p>Mã xác thực có hiệu lực trong vòng <strong>{OtpExpirationMinutes} phút</strong>. Tuyệt đối không chia sẻ mã này với bất kỳ ai.</p>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;" />
                <p style="font-size: 12px; color: #64748b; text-align: center;">Tillegal - Trợ lý pháp lý thông minh</p>
            </div>
            """;

        await emailService.SendAsync(normalizedEmail, emailSubject, htmlBody, cancellationToken);

        return new RegisterResult(
            Message: $"Verification code has been sent to {normalizedEmail}. Please verify within {OtpExpirationMinutes} minutes.",
            ExpirationMinutes: OtpExpirationMinutes);
    }
}
