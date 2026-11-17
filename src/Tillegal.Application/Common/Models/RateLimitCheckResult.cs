namespace Tillegal.Application.Common.Models;

public sealed record RateLimitCheckResult(
    bool IsAllowed,
    long CurrentCount,
    long Remaining,
    TimeSpan RetryAfter,
    DateTimeOffset ResetAt);
