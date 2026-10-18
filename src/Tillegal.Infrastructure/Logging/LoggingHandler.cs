using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Tillegal.Infrastructure.Logging;

public sealed class LoggingHandler(
    CorrelationIdGenerator correlationIdGenerator,
    ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.GetBaggageItem("correlationId")
            ?? Activity.Current?.TraceId.ToString()
            ?? correlationIdGenerator.Generate();
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

        logger.LogInformation(
            "Sending outbound HTTP {Method} request to {Host}. CorrelationId: {CorrelationId}",
            request.Method,
            request.RequestUri?.Host,
            correlationId);

        var response = await base.SendAsync(request, cancellationToken);

        logger.LogInformation(
            "Received outbound HTTP response {StatusCode}. CorrelationId: {CorrelationId}",
            (int)response.StatusCode,
            correlationId);

        return response;
    }
}
