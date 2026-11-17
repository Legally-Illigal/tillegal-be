using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.ExternalServices.RateLimiting;

public sealed class UpstashRedisRateLimitService : IRateLimitService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly ILogger<UpstashRedisRateLimitService> _logger;

    public UpstashRedisRateLimitService(
        HttpClient httpClient,
        IOptions<RedisOptions> options,
        ILogger<UpstashRedisRateLimitService> logger)
    {
        _logger = logger;
        var redisOptions = options.Value;
        if (!Uri.TryCreate(redisOptions.RedisRestUrl, UriKind.Absolute, out var baseAddress)
            || baseAddress.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(baseAddress.Query)
            || !string.IsNullOrEmpty(baseAddress.Fragment))
        {
            throw new InvalidOperationException(
                "Configure 'Upstash:RedisRestUrl' with the HTTPS REST URL from the Upstash console.");
        }

        _token = !string.IsNullOrWhiteSpace(redisOptions.RedisRestToken)
            ? redisOptions.RedisRestToken
            : throw new InvalidOperationException(
                "Configure 'Upstash:RedisRestToken' with the REST token from the Upstash console.");

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(baseAddress.AbsoluteUri.TrimEnd('/') + "/");
    }

    public async Task<RateLimitCheckResult> CheckRateLimitAsync(
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(permitLimit);
        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), "Window must be positive.");
        }

        var now = DateTimeOffset.UtcNow;
        var nowSeconds = now.ToUnixTimeSeconds();
        var windowSeconds = Math.Max(1, (long)window.TotalSeconds);
        var currentBucket = nowSeconds / windowSeconds;
        var resetSeconds = (currentBucket + 1) * windowSeconds;
        var resetAt = DateTimeOffset.FromUnixTimeSeconds(resetSeconds);
        var retryAfter = TimeSpan.FromSeconds(Math.Max(1, resetSeconds - nowSeconds));

        var redisKey = $"ratelimit:{partitionKey}:{currentBucket}";

        try
        {
            using var incrResponse = await SendCommandAsync(["INCR", redisKey], cancellationToken);
            var incrResult = await ReadResultAsync(incrResponse, cancellationToken);
            var currentCount = incrResult.GetInt64();

            if (currentCount <= 2)
            {
                var expireSeconds = (windowSeconds * 2).ToString(CultureInfo.InvariantCulture);
                using var expireResponse = await SendCommandAsync(["EXPIRE", redisKey, expireSeconds], cancellationToken);
            }

            var isAllowed = currentCount <= permitLimit;
            var remaining = Math.Max(0, permitLimit - currentCount);

            return new RateLimitCheckResult(
                IsAllowed: isAllowed,
                CurrentCount: currentCount,
                Remaining: remaining,
                RetryAfter: isAllowed ? TimeSpan.Zero : retryAfter,
                ResetAt: resetAt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute Redis rate limit command for key {RedisKey}. Failing open.", redisKey);
            return new RateLimitCheckResult(
                IsAllowed: true,
                CurrentCount: 1,
                Remaining: permitLimit - 1,
                RetryAfter: TimeSpan.Zero,
                ResetAt: resetAt);
        }
    }

    private async Task<HttpResponseMessage> SendCommandAsync(
        string[] command,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
        {
            Content = JsonContent.Create(command)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            response.Dispose();
            throw new HttpRequestException(
                $"Upstash REST command failed with HTTP status {(int)statusCode} ({statusCode}).");
        }

        return response;
    }

    private static async Task<JsonElement> ReadResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("result", out var result))
        {
            throw new InvalidOperationException("Upstash REST response did not include a result.");
        }

        return result.Clone();
    }
}
