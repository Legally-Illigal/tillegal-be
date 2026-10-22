using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;

namespace Tillegal.Api.Services;

public sealed class AuditLogBackgroundService(
    IAuditLogService auditLogService,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditLogBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (auditLogService is not AuditLogQueue auditLogQueue)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (await auditLogQueue.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (auditLogQueue.Reader.TryRead(out var entry))
                    {
                        await ProcessEntryAsync(entry, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred in AuditLogBackgroundService.");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task ProcessEntryAsync(AuditLogEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var writer = scope.ServiceProvider.GetService<IAuditLogWriter>();
            if (writer is not null)
            {
                await writer.WriteAsync(entry, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist audit log entry {AuditLogId} for action {Action}.", entry.Id, entry.Action);
        }
    }
}
