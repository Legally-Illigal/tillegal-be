namespace Tillegal.API.Common;

public sealed class RateLimitingOptions
{
    public int GlobalPermitLimit { get; init; } = 120;
    public int GlobalWindowSeconds { get; init; } = 60;
    public int AuthPermitLimit { get; init; } = 10;
    public int AuthWindowSeconds { get; init; } = 60;
}
