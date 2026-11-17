using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Infrastructure.ExternalServices.Jobs;

public sealed class DailyCrawlUpdateTask(
    IAiServiceClient aiServiceClient,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration,
    ILogger<DailyCrawlUpdateTask> logger) : IDailyUpdateTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var timeZoneId = configuration["BackgroundJobs:TimeZone"] ?? "Asia/Ho_Chi_Minh";
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var currentLocalTime = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, timeZone);

        // Daily crawl targets exactly 1 day prior to the current date
        var targetDate = DateOnly.FromDateTime(currentLocalTime.DateTime).AddDays(-1);

        logger.LogInformation(
            "Executing daily crawl task via AI Service. Current local time: {CurrentLocalTime} ({TimeZoneId}), Target crawl date: {TargetDate}",
            currentLocalTime,
            timeZoneId,
            targetDate);

        try
        {
            var jobId = await aiServiceClient.TriggerCrawlAsync(targetDate, cancellationToken);
            logger.LogInformation("Scheduled crawl job successfully dispatched to AI Service. Job ID: {JobId}", jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute daily crawl task for date {TargetDate}", targetDate);
            throw;
        }
    }
}
