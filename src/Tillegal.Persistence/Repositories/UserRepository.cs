using Microsoft.EntityFrameworkCore;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Entities;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> FindByEmailOrUserNameAsync(string emailOrUserName, CancellationToken cancellationToken = default)
    {
        var normalized = emailOrUserName.Trim();
        var userModel = await dbContext.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == normalized.ToLower()) ||
                (u.UserName != null && u.UserName.ToLower() == normalized.ToLower()),
                cancellationToken);

        return userModel is null ? null : ToDomain(userModel);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userModel = await dbContext.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return userModel is null ? null : ToDomain(userModel);
    }

    public async Task UpdateLastActiveAtAsync(Guid userId, DateTimeOffset lastActiveAt, CancellationToken cancellationToken = default)
    {
        var userModel = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (userModel is not null)
        {
            userModel.UpdatedAt = lastActiveAt;
        }
    }

    private static User ToDomain(UserDataModel model)
    {
        return new User(
            model.Id,
            model.RoleId,
            model.FullName,
            model.UserName,
            model.Email,
            model.PasswordHash,
            model.PhoneNumber,
            model.CreatedAt,
            model.UpdatedAt,
            model.Role is null ? null : new Role(model.Role.Id, model.Role.Name));
    }
}
