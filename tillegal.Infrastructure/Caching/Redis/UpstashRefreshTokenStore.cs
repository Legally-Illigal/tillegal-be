using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;
using Tillegal.Application.Authentication;

namespace Tillegal.Infrastructure.Caching.Redis;

public sealed class UpstashRefreshTokenStore(
    IConnectionMultiplexer connection,
    RefreshTokenStoreOptions options) : IRefreshTokenStore
{
    private const string KeyPrefix = "tillegal:refresh:";
    private const string ConsumeTokenScript = "local userId = redis.call('GET', KEYS[1]); if not userId then return nil end; redis.call('DEL', KEYS[1]); return userId";
    private readonly IDatabase _database = connection.GetDatabase();

    public async Task<RefreshTokenSession> IssueAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string token;
        do
        {
            token = CreateToken();
        }
        while (!await _database.StringSetAsync(
                   GetKey(token),
                   userId,
                   options.Lifetime,
                   When.NotExists));

        return new RefreshTokenSession(
            userId,
            token,
            DateTimeOffset.UtcNow.Add(options.Lifetime));
    }

    public async Task<RefreshTokenSession?> RotateAsync(
        string token,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _database.ScriptEvaluateAsync(
            ConsumeTokenScript,
            new RedisKey[] { GetKey(token) });
        var userId = (string?)result;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await IssueAsync(userId, cancellationToken);
    }

    public async Task RevokeAsync(
        string token,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.KeyDeleteAsync(GetKey(token));
    }

    private static RedisKey GetKey(string token) =>
        KeyPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string CreateToken()
    {
        var value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return value.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
