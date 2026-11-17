namespace Tillegal.Application.Common.Interfaces;

public interface IAiServiceClient
{
    Task<string?> TriggerCrawlAsync(DateOnly crawlDate, CancellationToken cancellationToken = default);
}
