using System.Security.Cryptography;
using System.Text.Json;
using MediatR;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Auth.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Auth.Commands.ResendOtp;

public sealed class ResendOtpCommandHandler(
    IAuthService authService,
    ICacheService cacheService,
    IEmailService emailService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<ResendOtpCommand, ResendOtpResult>
{
    private const int OtpExpirationMinutes = 10;

    public async Task<ResendOtpResult> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cacheKey = $"pending_reg:{normalizedEmail}";

        var cachedBytes = await cacheService.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is null || cachedBytes.Length == 0)
        {
            throw new DomainException("No pending registration found for this email or it has expired. Please register again.");
        }

        var pendingRegistration = JsonSerializer.Deserialize<PendingRegistration>(cachedBytes);
        if (pendingRegistration is null)
        {
            throw new DomainException("Invalid registration data. Please register again.");
        }

        // Check if user has already been registered in DB in the meantime
        await authService.ValidateEmailAvailabilityAsync(normalizedEmail, cancellationToken);

        // Generate a new OTP
        var newOtpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var updatedRegistration = pendingRegistration with
        {
            OtpCode = newOtpCode,
            CreatedAt = dateTimeProvider.UtcNow
        };

        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(updatedRegistration);

        // Reset TTL in cache
        await cacheService.SetAsync(
            cacheKey,
            payloadBytes,
            TimeSpan.FromMinutes(OtpExpirationMinutes),
            cancellationToken);

        // Send OTP email
        var emailSubject = "Mã xác thực đăng ký tài khoản Tillegal (Gửi lại)";
        var htmlBody = $"""
            <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;">
                <h2 style="color: #1e293b; text-align: center;">Mã xác thực mới tài khoản Tillegal</h2>
                <p>Xin chào <strong>{updatedRegistration.FullName}</strong>,</p>
                <p>Bạn đã yêu cầu gửi lại mã xác thực (OTP). Vui lòng sử dụng mã bên dưới để hoàn tất đăng ký:</p>
                <div style="margin: 24px 0; text-align: center;">
                    <span style="display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #2563eb; background-color: #eff6ff; padding: 12px 24px; border-radius: 6px; border: 1px dashed #93c5fd;">
                        {newOtpCode}
                    </span>
                </div>
                <p>Mã xác thực có hiệu lực trong vòng <strong>{OtpExpirationMinutes} phút</strong>. Tuyệt đối không chia sẻ mã này với bất kỳ ai.</p>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;" />
                <p style="font-size: 12px; color: #64748b; text-align: center;">Tillegal - Trợ lý pháp lý thông minh</p>
            </div>
            """;

        await emailService.SendAsync(normalizedEmail, emailSubject, htmlBody, cancellationToken);

        return new ResendOtpResult(
            Message: $"A new verification code has been sent to {normalizedEmail}. Please verify within {OtpExpirationMinutes} minutes.",
            ExpirationMinutes: OtpExpirationMinutes);
    }
}
