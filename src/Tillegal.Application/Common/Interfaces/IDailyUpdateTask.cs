namespace Tillegal.Application.Common.Interfaces;

public interface IDailyUpdateTask
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
