using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Infrastructure.Options;

namespace Tillegal.Infrastructure.ExternalServices.Storage;

public sealed class SupabaseStorageService(HttpClient httpClient, IOptions<SupabaseOptions> options) : IStorageService
{
    private readonly SupabaseOptions _options = options.Value;

    public async Task UploadAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        using var request = CreateRequest(HttpMethod.Post, objectKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<StoredObject?> DownloadAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, objectKey);
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        return new StoredObject(content, contentType);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, objectKey);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string objectKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        if (objectKey.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("Object keys must contain valid path segments.", nameof(objectKey));
        }

        var projectUrl = _options.Url.TrimEnd('/');
        var bucket = Uri.EscapeDataString(_options.Bucket);
        var escapedKey = string.Join('/', objectKey.Split('/').Select(Uri.EscapeDataString));
        var serviceKey = _options.ServiceKey;
        var request = new HttpRequestMessage(method, $"{projectUrl}/storage/v1/object/{bucket}/{escapedKey}");
        request.Headers.TryAddWithoutValidation("apikey", serviceKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
        return request;
    }
}
