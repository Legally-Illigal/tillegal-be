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
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey) && options.SigningKey.Length >= 32,
                "JWT SigningKey must be at least 32 characters long.")
            .ValidateOnStart();

        return services.AddInfrastructure(configuration);
    }
}
