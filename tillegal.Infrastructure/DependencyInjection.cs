using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tillegal.Application.Authentication;
using StackExchange.Redis;
using Tillegal.Application.Common.Abstractions;
using Tillegal.Infrastructure.Authentication;
using Tillegal.Infrastructure.Caching.Redis;
using Tillegal.Infrastructure.Messaging.RabbitMQ;
using Tillegal.Infrastructure.Persistence.PostgreSQL;
using Tillegal.Infrastructure.Storage.Supabase;
namespace Tillegal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is required. Configure it using User Secrets or an environment variable.");
        }

        var supabaseOptions = configuration.GetSection("Supabase").Get<SupabaseStorageOptions>();
        if (supabaseOptions is null
            || !Uri.TryCreate(supabaseOptions.Url, UriKind.Absolute, out var supabaseUri)
            || supabaseUri.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrWhiteSpace(supabaseOptions.ServiceRoleKey)
            || string.IsNullOrWhiteSpace(supabaseOptions.BucketName))
        {
            throw new InvalidOperationException(
                "Supabase:Url, Supabase:ServiceRoleKey and Supabase:BucketName must be configured with an HTTPS project URL and private bucket.");
        }

        var rabbitOptions = configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>();
        if (rabbitOptions is null
            || !Uri.TryCreate(rabbitOptions.Uri, UriKind.Absolute, out var rabbitUri)
            || rabbitUri.Scheme != "amqps"
            || string.IsNullOrWhiteSpace(rabbitOptions.ExchangeName))
        {
            throw new InvalidOperationException(
                "RabbitMQ:Uri must be an AMQPS CloudAMQP URI and RabbitMQ:ExchangeName must be configured.");
        }

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException(
                "Redis:ConnectionString is required for refresh-token storage.");
        }

        var refreshTokenDays = configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7;
        if (refreshTokenDays is < 1 or > 90)
        {
            throw new InvalidOperationException(
                "Jwt:RefreshTokenDays must be between 1 and 90.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton(new RefreshTokenStoreOptions(
            TimeSpan.FromDays(refreshTokenDays)));
        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IRefreshTokenStore, UpstashRefreshTokenStore>();
        services.Configure<SupabaseStorageOptions>(configuration.GetSection("Supabase"));
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddHttpClient<IObjectStorage, SupabaseObjectStorage>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<SupabaseStorageOptions>>().Value;
            client.BaseAddress = new Uri(options.Url.TrimEnd('/') + "/storage/v1/");
            client.DefaultRequestHeaders.TryAddWithoutValidation("apikey", options.ServiceRoleKey);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ServiceRoleKey);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHttpClient("SupabaseStorageHealth", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<SupabaseStorageOptions>>().Value;
            client.BaseAddress = new Uri(options.Url.TrimEnd('/') + "/storage/v1/");
            client.DefaultRequestHeaders.TryAddWithoutValidation("apikey", options.ServiceRoleKey);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ServiceRoleKey);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
