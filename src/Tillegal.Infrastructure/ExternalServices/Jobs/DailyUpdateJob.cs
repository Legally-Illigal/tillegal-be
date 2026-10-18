using Microsoft.Extensions.Logging;
using Quartz;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Infrastructure.ExternalServices.Jobs;

[DisallowConcurrentExecution]
public sealed class DailyUpdateJob(
    IEnumerable<IDailyUpdateTask> tasks,
    ILogger<DailyUpdateJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var updateTasks = tasks.ToArray();
        if (updateTasks.Length == 0)
        {
            logger.LogWarning("The daily update job ran, but no {TaskType} handlers are registered.", nameof(IDailyUpdateTask));
            return;
        }

        foreach (var task in updateTasks)
        {
            await task.ExecuteAsync(cancellationToken);
        }
    }
}
