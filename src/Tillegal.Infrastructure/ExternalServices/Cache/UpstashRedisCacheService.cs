using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.ExternalServices.Cache;

public sealed class UpstashRedisCacheService : ICacheService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public UpstashRedisCacheService(HttpClient httpClient, IOptions<RedisOptions> options)
    {
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

    public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var response = await SendCommandAsync(["GET", key], cancellationToken);
        var result = await ReadResultAsync(response, cancellationToken);
        if (result.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var encodedValue = result.GetString()
            ?? throw new InvalidOperationException("Upstash returned a non-string value for a cache GET command.");
        return Convert.FromBase64String(encodedValue);
    }

    public async Task SetAsync(
        string key,
        byte[] value,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        string[] command;
        var encodedValue = Convert.ToBase64String(value);
        if (absoluteExpiration is { } expiration)
        {
            if (expiration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(absoluteExpiration), "Expiration must be positive.");
            }

            var milliseconds = checked((long)Math.Ceiling(expiration.TotalMilliseconds));
            command = ["SET", key, encodedValue, "PX", milliseconds.ToString(CultureInfo.InvariantCulture)];
        }
        else
        {
            command = ["SET", key, encodedValue];
        }

        using var response = await SendCommandAsync(command, cancellationToken);
        var result = await ReadResultAsync(response, cancellationToken);
        if (result.GetString() != "OK")
        {
            throw new InvalidOperationException("Upstash did not confirm the cache SET command.");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var response = await SendCommandAsync(["DEL", key], cancellationToken);
        var result = await ReadResultAsync(response, cancellationToken);
        if (result.ValueKind != JsonValueKind.Number || !result.TryGetInt64(out _))
        {
            throw new InvalidOperationException("Upstash returned an invalid result for a cache DEL command.");
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
