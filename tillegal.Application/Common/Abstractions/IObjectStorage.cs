namespace Tillegal.Application.Common.Abstractions;

public interface IObjectStorage
{
    Task UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string objectPath,
        CancellationToken cancellationToken);

    Task<Uri> CreateSignedDownloadUrlAsync(
        string objectPath,
        TimeSpan expiresIn,
        CancellationToken cancellationToken);
}
