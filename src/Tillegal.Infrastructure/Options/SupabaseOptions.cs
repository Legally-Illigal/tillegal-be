namespace Tillegal.Infrastructure.Options;

public sealed class SupabaseOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public string ServiceKey { get; set; } = string.Empty;
}
