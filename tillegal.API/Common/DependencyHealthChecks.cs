using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using StackExchange.Redis;

using Tillegal.Infrastructure.Caching.Redis;
using Tillegal.Infrastructure.Messaging.RabbitMQ;
using Tillegal.Infrastructure.Persistence.PostgreSQL;
using Tillegal.Infrastructure.Storage.Supabase;

namespace Tillegal.API.Common;

public sealed class PostgreSqlHealthCheck(ApplicationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
        }
    }
}

public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await redis.GetDatabase().PingAsync().WaitAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable.");
        }
    }
}

public sealed class RabbitMqHealthCheck(RabbitMqConnection rabbitMq) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IConnection connection = await rabbitMq.GetConnectionAsync(cancellationToken);
            return connection.IsOpen
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("RabbitMQ is unavailable.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("RabbitMQ is unavailable.");
        }
    }
}

public sealed class SupabaseStorageHealthCheck(
    IHttpClientFactory httpClientFactory,
    IOptions<SupabaseStorageOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpClient client = httpClientFactory.CreateClient("SupabaseStorageHealth");
            var bucket = Uri.EscapeDataString(options.Value.BucketName);
            using HttpResponseMessage response = await client.GetAsync(
                $"bucket/{bucket}",
                cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Supabase Storage is unavailable.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Supabase Storage is unavailable.");
        }
    }
}

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
            }),
        };
        return JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            cancellationToken: context.RequestAborted);
    }
}
