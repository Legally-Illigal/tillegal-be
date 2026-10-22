using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Infrastructure.ExternalServices.Cache;
using Tillegal.Infrastructure.ExternalServices.Jobs;
using Tillegal.Infrastructure.ExternalServices.Mail;
using Tillegal.Infrastructure.ExternalServices.Storage;
using Tillegal.Infrastructure.Logging;
using Tillegal.Infrastructure.SystemServices;

namespace Tillegal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
        services.AddSingleton<CorrelationIdGenerator>();
        services.AddOptions<Options.JwtOptions>()
            .Bind(configuration.GetSection(Options.JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddTransient<LoggingHandler>();
        services.AddHttpClient<ICacheService, UpstashRedisCacheService>()
            .AddHttpMessageHandler<LoggingHandler>();
        services.AddHttpClient<IEmailService, ResendEmailService>()
            .AddHttpMessageHandler<LoggingHandler>();
        services.AddHttpClient<IStorageService, SupabaseStorageService>()
            .AddHttpMessageHandler<LoggingHandler>();

        var timeZoneId = configuration["BackgroundJobs:TimeZone"] ?? "Asia/Ho_Chi_Minh";
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        services.AddQuartz(quartz =>
        {
            var jobKey = new JobKey("daily-update");
            quartz.AddJob<DailyUpdateJob>(options => options.WithIdentity(jobKey));
            quartz.AddTrigger(options => options
                .ForJob(jobKey)
                .WithIdentity("daily-update-at-midnight")
                .WithCronSchedule("0 0 0 * * ?", schedule => schedule.InTimeZone(timeZone)));
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}
