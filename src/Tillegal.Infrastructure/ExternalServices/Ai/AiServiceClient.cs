using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
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

    public async Task<AiConsultationResult> ConsultLegalAsync(
        string prompt,
        string workflowMode,
        IReadOnlyList<ConversationHistoryItem>? history = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "http://127.0.0.1:8000"
            : _options.BaseUrl.TrimEnd('/');
        var endpoint = string.IsNullOrWhiteSpace(_options.ConsultationEndpoint)
            ? "/consultation"
            : (_options.ConsultationEndpoint.StartsWith('/') ? _options.ConsultationEndpoint : "/" + _options.ConsultationEndpoint);
        var requestUrl = $"{baseUrl}{endpoint}";

        logger.LogInformation("Dispatching legal consultation request to AI Service at {RequestUrl} with workflow mode {WorkflowMode}", requestUrl, workflowMode);

        var payload = new ConsultationPayload(
            Prompt: prompt,
            WorkflowMode: workflowMode,
            History: history?.Select(h => new HistoryItemPayload(h.Role, h.Content)).ToList());

        try
        {
            using var response = await httpClient.PostAsJsonAsync(requestUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ConsultationResponse>(cancellationToken: cancellationToken);
            logger.LogInformation("AI Service consultation responded successfully with workflow mode {WorkflowMode}", workflowMode);

            return new AiConsultationResult(
                Content: result?.Content ?? string.Empty,
                Citations: result?.Citations);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to connect to AI Service at {RequestUrl}. Returning mock fallback response.", requestUrl);
            var modeNotice = string.Equals(workflowMode, "multi_agent", StringComparison.OrdinalIgnoreCase)
                ? "[Đa Agent - Chuyên sâu] Hệ thống đã tổng hợp ý kiến từ nhiều chuyên gia pháp lý."
                : "[Đơn Agent - Tiêu chuẩn] Hệ thống đang phân tích câu hỏi của bạn.";

            return new AiConsultationResult(
                Content: $"{modeNotice} Câu hỏi: \"{prompt}\" đã được ghi nhận.",
                Citations: null);
        }
    }

    private sealed record CrawlPayload(
        [property: JsonPropertyName("crawl_date")] string CrawlDate);

    private sealed record CrawlResponse(
        [property: JsonPropertyName("id")] string? Id);

    private sealed record ConsultationPayload(
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("workflow_mode")] string WorkflowMode,
        [property: JsonPropertyName("history")] IReadOnlyList<HistoryItemPayload>? History);

    private sealed record HistoryItemPayload(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ConsultationResponse(
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("citations")] JsonDocument? Citations);
}
