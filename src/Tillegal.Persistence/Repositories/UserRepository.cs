namespace Tillegal.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataModel = await dbContext.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var dataModel = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<User?> GetByGoogleSubjectAsync(string googleSubject, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(googleSubject);

        var dataModel = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.GoogleSubject == googleSubject, cancellationToken);

        return dataModel is null ? null : MapToEntity(dataModel);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await dbContext.Users
            .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<Role?> GetDefaultRoleAsync(string roleName = "User", CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = roleName.Trim().ToLowerInvariant();
        var dataModel = await dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == normalizedRoleName, cancellationToken);

        return dataModel is null ? null : new Role(dataModel.Id, dataModel.Name);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var dataModel = new UserDataModel
        {
            Id = user.Id,
            RoleId = user.RoleId,
            FullName = user.FullName,
            Email = user.Email,
            GoogleSubject = user.GoogleSubject,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            DeletedAt = user.IsActive ? null : DateTimeOffset.UtcNow
        };

        await dbContext.Users.AddAsync(dataModel, cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(normalized)) ||
                (u.Email != null && u.Email.ToLower().Contains(normalized)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var dataModels = await query
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = dataModels.Select(MapToEntity).ToList();
        return (items, totalCount);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var dataModel = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (dataModel is null)
            return;

        dataModel.DeletedAt = user.IsActive ? null : DateTimeOffset.UtcNow;
    }

    private static User MapToEntity(UserDataModel dataModel)
    {
        return new User
        {
            Id = dataModel.Id,
            RoleId = dataModel.RoleId,
            FullName = dataModel.FullName,
            Email = dataModel.Email,
            GoogleSubject = dataModel.GoogleSubject,
            PasswordHash = dataModel.PasswordHash,
            PhoneNumber = dataModel.PhoneNumber,
            CreatedAt = dataModel.CreatedAt,
            Role = dataModel.Role != null ? new Role(dataModel.Role.Id, dataModel.Role.Name) : null,
            IsActive = dataModel.DeletedAt == null
        };
    }
}
