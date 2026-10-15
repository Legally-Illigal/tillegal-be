namespace Tillegal.Application.Common.Abstractions
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(
            CancellationToken cancellationToken);

        Task SaveChangesAsync(
            CancellationToken cancellationToken);

        Task CommitAsync(
            CancellationToken cancellationToken);

        Task RollbackAsync(
            CancellationToken cancellationToken);
    }
}
