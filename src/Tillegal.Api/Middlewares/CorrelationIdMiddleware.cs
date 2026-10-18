using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Tillegal.Api.Middlewares;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";
    private const string BaggageKey = "correlationId";

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = Activity.Current is null
            ? new Activity("HTTP Request").SetIdFormat(ActivityIdFormat.W3C).Start()
            : null;

        var correlationId = GetCorrelationId(context.Request.Headers[HeaderName].ToString())
            ?? Activity.Current!.TraceId.ToString();
        context.TraceIdentifier = correlationId;
        Activity.Current?.AddBaggage(BaggageKey, correlationId);
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }

    private static string? GetCorrelationId(string value)
    {
        if (value.Length is 0 or > 128)
        {
            return null;
        }

        foreach (var character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_' and not '.')
            {
                return null;
            }
        }

        return value;
    }
}
