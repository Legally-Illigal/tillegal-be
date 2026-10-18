namespace Tillegal.Infrastructure.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Upstash";

    public string RedisRestUrl { get; set; } = string.Empty;

    public string RedisRestToken { get; set; } = string.Empty;
}
