using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tillegal.Infrastructure.Options;
using Tillegal.Infrastructure;

namespace Tillegal.AppHost.Configurations;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.RedisRestUrl, UriKind.Absolute, out var url)
                && url.Scheme == Uri.UriSchemeHttps
                && string.IsNullOrWhiteSpace(url.Query)
                && string.IsNullOrWhiteSpace(url.Fragment),
                "Upstash Redis REST URL must be an HTTPS URL without query or fragment.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.RedisRestToken),
                "Upstash Redis REST token is required.")
            .ValidateOnStart();

        services.AddOptions<ResendOptions>()
            .Bind(configuration.GetSection(ResendOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "Resend API key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.FromEmail), "Resend sender email is required.")
            .ValidateOnStart();

        services.AddOptions<SupabaseOptions>()
            .Bind(configuration.GetSection(SupabaseOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.Url, UriKind.Absolute, out var url)
                && url.Scheme == Uri.UriSchemeHttps
                && string.IsNullOrWhiteSpace(url.Query)
                && string.IsNullOrWhiteSpace(url.Fragment),
                "Supabase URL must be an HTTPS URL without query or fragment.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Bucket), "Supabase storage bucket is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ServiceKey), "Supabase service key is required.")
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
https://github.com/Legally-Illigal/tillegal-be/pull/29/conflict?name=src%252FTillegal.AppHost%252FConfigurations%252FInfrastructureExtensions.cs&ancestor_oid=07a27b69ff9f4686afa191e86439cfcbbcbf2057&base_oid=6c873d2e41c6df75244c2e787a78bcf7b610c552&head_oid=45dd2347b07c8af1bb091fba58801811261709aa            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey) && options.SigningKey.Length >= 32,
                "JWT SigningKey must be at least 32 characters long.")
            .ValidateOnStart();

        services.AddOptions<AiServiceOptions>()
            .Bind(configuration.GetSection(AiServiceOptions.SectionName));

        return services.AddInfrastructure(configuration);
    }
}
