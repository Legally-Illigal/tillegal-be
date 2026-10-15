using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text;
using Tillegal.API.Models;
using Tillegal.Infrastructure.Authentication;

namespace Tillegal.API.Common;

public static class AuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection("Jwt")
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is required. Configure Jwt:Issuer, Jwt:Audience, and Jwt:Key.");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        var sameSite = configuration["Authentication:RefreshTokenCookie:SameSite"] ?? "Lax";
        if (!Enum.TryParse<SameSiteMode>(sameSite, true, out _))
        {
            throw new InvalidOperationException(
                "Authentication:RefreshTokenCookie:SameSite must be Strict, Lax, or None.");
        }

        services.Configure<RefreshTokenCookieOptions>(
            configuration.GetSection("Authentication:RefreshTokenCookie"));

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
            || string.IsNullOrWhiteSpace(jwtOptions.Audience)
            || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32
            || jwtOptions.AccessTokenMinutes is < 1 or > 60)
        {
            throw new InvalidOperationException(
                "JWT issuer and audience are required, Jwt:Key must contain at least 32 UTF-8 bytes, and Jwt:AccessTokenMinutes must be between 1 and 60.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)),

                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    // Lấy JWT từ Cookie
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(
                            "rescuehub_token",
                            out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },

                    // 401 - Chưa đăng nhập / Token không hợp lệ
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        var response = ApiResponse.Fail(
                            HttpStatusCode.Unauthorized,
                            new[]
                            {
                                "Bạn chưa đăng nhập hoặc token không hợp lệ."
                            });

                        await context.Response.WriteAsJsonAsync(
                            response);
                    },

                    // 403 - Đã đăng nhập nhưng không đủ quyền
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        var response = ApiResponse.Fail(
                            HttpStatusCode.Forbidden,
                            new[]
                            {
                                "Bạn không có quyền truy cập chức năng này."
                            });

                        await context.Response.WriteAsJsonAsync(
                            response);
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}