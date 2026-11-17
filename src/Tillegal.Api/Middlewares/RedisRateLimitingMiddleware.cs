using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tillegal.Api.Options;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Api.Middlewares;

public sealed class RedisRateLimitingMiddleware(
    RequestDelegate next,
    ILogger<RedisRateLimitingMiddleware> logger,
    IOptions<RateLimitingOptions> options)
{
    private static readonly PathString ApiPrefix = "/api";

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        // Only rate-limit API routes
        if (!context.Request.Path.StartsWithSegments(ApiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Bypass CORS preflight requests
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await next(context);
            return;
        }

        var rateLimitOptions = options.Value ?? new RateLimitingOptions();
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

        string partitionKey;
        int permitLimit;
        TimeSpan window;

        if (IsLoginEndpoint(context.Request.Path))
        {
            var loginPolicy = rateLimitOptions.Login ?? new RateLimitPolicyOptions();
            permitLimit = Math.Max(1, loginPolicy.PermitLimit);
            window = TimeSpan.FromSeconds(Math.Max(1, loginPolicy.WindowSeconds));
            partitionKey = $"login:{clientIp}";
        }
        else
        {
            var globalPolicy = rateLimitOptions.Global ?? new RateLimitPolicyOptions();
            permitLimit = Math.Max(1, globalPolicy.PermitLimit);
            window = TimeSpan.FromSeconds(Math.Max(1, globalPolicy.WindowSeconds));

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");

            partitionKey = !string.IsNullOrWhiteSpace(userId)
                ? $"global:user:{userId}"
                : $"global:ip:{clientIp}";
        }

        try
        {
            var checkResult = await rateLimitService.CheckRateLimitAsync(
                partitionKey,
                permitLimit,
                window,
                context.RequestAborted);

            context.Response.Headers["X-RateLimit-Limit"] = permitLimit.ToString(CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, checkResult.Remaining).ToString(CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Reset"] = checkResult.ResetAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

            if (!checkResult.IsAllowed)
            {
                var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(checkResult.RetryAfter.TotalSeconds));

                logger.LogWarning(
                    "Rate limit exceeded for {PartitionKey} on {Path}. Limit: {Limit}, RetryAfter: {RetryAfter}s. TraceId: {TraceId}",
                    partitionKey,
                    context.Request.Path,
                    permitLimit,
                    retryAfterSeconds,
                    context.TraceIdentifier);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Detail = $"Rate limit exceeded. Please try again in {retryAfterSeconds} seconds.",
                    Type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4"
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;

                await JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    problem,
                    cancellationToken: context.RequestAborted);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error evaluating rate limit for {PartitionKey}. Allowing request to proceed. TraceId: {TraceId}",
                partitionKey,
                context.TraceIdentifier);
        }

        await next(context);
    }

    private static bool IsLoginEndpoint(PathString path) =>
        path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/auth/login/google", StringComparison.OrdinalIgnoreCase);
}
