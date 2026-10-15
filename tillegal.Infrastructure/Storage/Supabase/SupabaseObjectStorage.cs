using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Tillegal.Application.Common.Abstractions;

namespace Tillegal.Infrastructure.Storage.Supabase;

public sealed class SupabaseObjectStorage(
    HttpClient httpClient,
    IOptions<SupabaseStorageOptions> options) : IObjectStorage
{
    private readonly SupabaseStorageOptions _options = options.Value;

    public async Task UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        ValidateObjectPath(objectPath);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            BuildObjectRoute(objectPath));
        request.Headers.TryAddWithoutValidation("x-upsert", "false");
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(
        string objectPath,
        CancellationToken cancellationToken)
    {
        ValidateObjectPath(objectPath);

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"object/{Uri.EscapeDataString(_options.BucketName)}")
        {
            Content = JsonContent.Create(new { prefixes = new[] { objectPath } })
        };
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Uri> CreateSignedDownloadUrlAsync(
        string objectPath,
        TimeSpan expiresIn,
        CancellationToken cancellationToken)
    {
        ValidateObjectPath(objectPath);
        if (expiresIn <= TimeSpan.Zero || expiresIn > TimeSpan.FromDays(7))
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresIn),
                "Signed URL expiration must be greater than zero and at most seven days.");
        }

        var route = $"object/sign/{Uri.EscapeDataString(_options.BucketName)}/{EscapeObjectPath(objectPath)}";
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            route,
            new { expiresIn = (long)expiresIn.TotalSeconds },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        SignedUrlResponse result = await response.Content.ReadFromJsonAsync<SignedUrlResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Supabase returned an empty signed URL response.");

        if (Uri.TryCreate(result.SignedUrl, UriKind.Absolute, out Uri? absoluteUri))
        {
            return absoluteUri;
        }

        var storageBaseUri = new Uri(_options.Url.TrimEnd('/') + "/storage/v1/");
        var signedPath = result.SignedUrl.TrimStart('/');
        if (!signedPath.StartsWith("object/", StringComparison.Ordinal))
        {
            signedPath = "object/" + signedPath;
        }

        return new Uri(storageBaseUri, signedPath);
    }

    private string BuildObjectRoute(string objectPath) =>
        $"object/{Uri.EscapeDataString(_options.BucketName)}/{EscapeObjectPath(objectPath)}";

    private static string EscapeObjectPath(string objectPath) =>
        string.Join('/', objectPath.Split('/').Select(Uri.EscapeDataString));

    private static void ValidateObjectPath(string objectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectPath);
        if (objectPath.StartsWith('/')
            || objectPath.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("Object path must be a relative path without empty or traversal segments.", nameof(objectPath));
        }
    }

    private sealed record SignedUrlResponse(
        [property: JsonPropertyName("signedURL")] string SignedUrl);
}
