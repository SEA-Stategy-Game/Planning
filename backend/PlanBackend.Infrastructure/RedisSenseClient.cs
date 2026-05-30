using PlanBackend.Application.Interfaces;
using StackExchange.Redis;

namespace PlanBackend.Infrastructure;

public class RedisSenseClient(IConnectionMultiplexer redisConnection) : ISenseQueryClient
{
    private readonly IDatabase _db = redisConnection.GetDatabase();

    public async Task<IReadOnlySet<string>> GetUnitIdsAsync(string gameId)
        => await FetchIdsAsync(gameId, "units");

    public async Task<IReadOnlySet<string>> GetResourceIdsAsync(string gameId)
        => await FetchIdsAsync(gameId, "resources");

    private async Task<IReadOnlySet<string>> FetchIdsAsync(string gameId, string arrayKey)
    {
        try
        {
            var members = await _db.SetMembersAsync($"game:{gameId}:{arrayKey}");
            return members
                .Select(v => v.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet()!;
        }
        catch
        {
            return new HashSet<string>();
        }
    }
}
