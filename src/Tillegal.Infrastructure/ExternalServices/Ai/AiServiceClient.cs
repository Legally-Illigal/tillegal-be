using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.ExternalServices.Ai;

public sealed class AiServiceClient(
    HttpClient httpClient,
    IOptions<AiServiceOptions> options,
    ILogger<AiServiceClient> logger) : IAiServiceClient
{
    private readonly AiServiceOptions _options = options.Value;

    public async Task<string?> TriggerCrawlAsync(DateOnly crawlDate, CancellationToken cancellationToken = default)
    {
        var formattedDate = crawlDate.ToString("yyyy-MM-dd");
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "http://127.0.0.1:8000"
            : _options.BaseUrl.TrimEnd('/');
        var requestUrl = $"{baseUrl}/ingestion/vbpl/crawl";

        logger.LogInformation("Dispatching crawl request to AI Service at {RequestUrl} for date {CrawlDate}", requestUrl, formattedDate);

        var payload = new CrawlPayload(formattedDate);
        using var response = await httpClient.PostAsJsonAsync(requestUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CrawlResponse>(cancellationToken: cancellationToken);
        logger.LogInformation("AI Service crawl responded with job ID {JobId} for date {CrawlDate}", result?.Id, formattedDate);

        return result?.Id;
    }

    private sealed record CrawlPayload(
        [property: JsonPropertyName("crawl_date")] string CrawlDate);

    private sealed record CrawlResponse(
        [property: JsonPropertyName("id")] string? Id);
}
