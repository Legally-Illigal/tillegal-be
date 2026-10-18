using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Tillegal.Api.Middlewares;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using var requestActivity = Activity.Current is null
            ? new Activity("HTTP Request").SetIdFormat(ActivityIdFormat.W3C).Start()
            : null;
        var correlationId = Activity.Current!.TraceId.ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        var stopwatch = Stopwatch.StartNew();
        Exception? requestException = null;

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            requestException = exception;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            if (requestException is not null)
            {
                logger.LogError(
                    requestException,
                    "HTTP {Method} {Path} failed after {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
            else
            {
                logger.LogInformation(
                    "HTTP {Method} {Path} responded with {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }
}
