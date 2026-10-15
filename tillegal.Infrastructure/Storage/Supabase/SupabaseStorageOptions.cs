namespace Tillegal.Infrastructure.Storage.Supabase;

public sealed class SupabaseStorageOptions
{
    public string Url { get; init; } = string.Empty;
    public string ServiceRoleKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
}
