using Tillegal.Application.Common.Models;

namespace Tillegal.Application.Common.Interfaces;

public interface IRateLimitService
{
    Task<RateLimitCheckResult> CheckRateLimitAsync(
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}
