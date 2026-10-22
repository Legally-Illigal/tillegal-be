using Tillegal.Domain.Entities;

namespace Tillegal.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailOrUserNameAsync(string emailOrUserName, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateLastActiveAtAsync(Guid userId, DateTimeOffset lastActiveAt, CancellationToken cancellationToken = default);
}
