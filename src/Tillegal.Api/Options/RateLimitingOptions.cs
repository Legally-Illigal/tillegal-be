namespace Tillegal.Api.Options;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions Global { get; set; } = new()
    {
        PermitLimit = 100,
        WindowSeconds = 60
    };

    public RateLimitPolicyOptions Login { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60
    };
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; } = 100;

    public int WindowSeconds { get; set; } = 60;
}
