namespace Tillegal.Application.Common.Interfaces;

public interface IStorageService
{
    Task UploadAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<StoredObject?> DownloadAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}

public sealed record StoredObject(byte[] Content, string ContentType);
