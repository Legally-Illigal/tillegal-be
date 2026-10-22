using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Common;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public DbSet<RoleDataModel> Roles => Set<RoleDataModel>();
    public DbSet<UserDataModel> Users => Set<UserDataModel>();
    public DbSet<AuditLogDataModel> AuditLogs => Set<AuditLogDataModel>();
    public DbSet<ConversationDataModel> Conversations => Set<ConversationDataModel>();
    public DbSet<ConversationMessageDataModel> ConversationMessages => Set<ConversationMessageDataModel>();
    public DbSet<UserAttachmentDataModel> UserAttachments => Set<UserAttachmentDataModel>();
    public DbSet<AiEvaluationLogDataModel> AiEvaluationLogs => Set<AiEvaluationLogDataModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplySoftDeleteQueryFilter(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetUpdatedTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException("A transaction is already active for this unit of work.");
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = _currentTransaction
            ?? throw new InvalidOperationException("There is no active transaction to commit.");

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = _currentTransaction
            ?? throw new InvalidOperationException("There is no active transaction to roll back.");

        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    private void SetUpdatedTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseDataModel>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.UpdatedAt).CurrentValue = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.UpdatedAt).CurrentValue = now;
            }
        }
    }

    private static void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseDataModel).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var property = Expression.Property(parameter, nameof(BaseDataModel.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTimeOffset?));
                var compare = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(compare, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            else if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTimeOffset?));
                var compare = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(compare, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
