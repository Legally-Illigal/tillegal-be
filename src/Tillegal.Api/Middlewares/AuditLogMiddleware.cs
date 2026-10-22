using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;

namespace Tillegal.Api.Middlewares;

public sealed class AuditLogMiddleware(
    RequestDelegate next,
    ILogger<AuditLogMiddleware> logger)
{
    private static readonly HashSet<string> AuditableHttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        var method = context.Request.Method;
        var isAuditableMethod = AuditableHttpMethods.Contains(method);

        if (!isAuditableMethod)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        // Skip health checks and OpenAPI endpoints
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var stopwatch = Stopwatch.StartNew();
        Exception? exceptionThrown = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            exceptionThrown = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = exceptionThrown is not null
                ? StatusCodes.Status500InternalServerError
                : context.Response.StatusCode;

            var status = statusCode is >= 200 and < 300 ? "SUCCESS" : "FAILED";

            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");
            Guid? actorUserId = Guid.TryParse(userIdClaim, out var parsedGuid) ? parsedGuid : null;
            var actorRole = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");

            var actionName = $"{method} {path}";
            var description = $"HTTP {method} {path} finished with status {statusCode} in {stopwatch.ElapsedMilliseconds}ms";

            var auditEntry = new AuditLogEntry
            {
                EventType = "HTTP_REQUEST",
                ActorUserId = actorUserId,
                ActorRole = actorRole,
                IpAddress = ipAddress,
                Action = actionName,
                Status = status,
                Description = description,
                CreatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                await auditLogService.EnqueueAsync(auditEntry);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to enqueue audit log for {Method} {Path}", method, path);
            }
        }
    }
}
