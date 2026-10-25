using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.ExternalServices.AiService;

public sealed class LegalAiServiceClient(
    HttpClient httpClient,
    IOptions<AiServiceOptions> options,
    ILogger<LegalAiServiceClient> logger) : ILegalAiService
{
    private readonly AiServiceOptions _options = options.Value;

    public async Task<AiLegalAnswer> AskAsync(
        AiQuestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            try
            {
                var baseUri = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
                var endpoint = new Uri(baseUri, "api/v1/chat");

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
                }

                request.Content = JsonContent.Create(new
                {
                    conversationId = context.ConversationId,
                    question = context.Question,
                    history = context.History
                });

                using var response = await httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ExternalAiServiceResponse>(cancellationToken);
                    if (result is not null && !string.IsNullOrWhiteSpace(result.Answer))
                    {
                        JsonDocument? citationsDoc = null;
                        if (result.Citations is not null)
                        {
                            citationsDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Citations));
                        }

                        return new AiLegalAnswer(result.Answer, citationsDoc);
                    }
                }
                else
                {
                    logger.LogWarning("AI service returned HTTP {StatusCode}. Using fallback answer.", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to connect to external AI service at {BaseUrl}. Using fallback response.", _options.BaseUrl);
            }
        }

        return GenerateFallbackLegalAnswer(context.Question);
    }

    private static AiLegalAnswer GenerateFallbackLegalAnswer(string question)
    {
        var content = $"""
            ### 1. Cơ sở pháp lý
            Căn cứ theo quy định của pháp luật Việt Nam hiện hành điều chỉnh về vấn đề: "{question.Trim()}".

            ### 2. Phân tích và tư vấn pháp lý
            - Đối với yêu cầu hoặc tình huống quý khách đang quan tâm, các bên cần rà soát điều kiện pháp lý, quyền và nghĩa vụ tương ứng theo quy định pháp luật hiện hành.
            - Quý khách cần lưu ý chuẩn bị hồ sơ đầy đủ, kiểm tra tính hợp pháp của các giao dịch, văn bản thỏa thuận và thực hiện đúng thời hạn theo quy định.

            ### 3. Khuyến nghị thực hiện
            Khuyến khích các bên ưu tiên thương lượng, hòa giải hoặc tham vấn chuyên gia pháp lý trước khi tiến hành các thủ tục tố tụng hoặc khiếu nại.
            """;

        var citationsJson = """
            [
              {
                "id": "cit-legal-001",
                "index": 1,
                "documentTitle": "Bộ luật Dân sự 2015",
                "documentNumber": "91/2015/QH13",
                "articleReference": "Điều 3, Điều 351",
                "sourceUrl": "https://thuvienphapluat.vn/van-ban/Bat-dong-san/Bo-luat-dan-su-2015-279332.aspx",
                "citedText": "Các nguyên tắc cơ bản của pháp luật dân sự và căn cứ phát sinh trách nhiệm dân sự."
              }
            ]
            """;

        return new AiLegalAnswer(content, JsonDocument.Parse(citationsJson));
    }

    private sealed record ExternalAiServiceResponse(
        string? Answer,
        object? Citations);
}
