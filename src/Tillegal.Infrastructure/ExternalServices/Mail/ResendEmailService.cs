using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Infrastructure.ExternalServices.Mail;

public sealed class ResendEmailService(HttpClient httpClient, IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNull(htmlBody);

        var apiKey = GetRequiredSetting("Resend:ApiKey");
        var from = GetRequiredSetting("Resend:FromEmail");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new { from, to = new[] { recipient }, subject, html = htmlBody });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private string GetRequiredSetting(string key) =>
        configuration[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Required configuration '{key}' is missing.");
}
