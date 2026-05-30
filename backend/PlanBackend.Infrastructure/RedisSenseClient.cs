using System.Text.Json;
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
            var jsonValue = await _db.StringGetAsync($"game:{gameId}:state_snapshot");
            
            if (jsonValue.IsNullOrEmpty)
            {
                // Key does not exist (TTL expired or game room down)
                return new HashSet<string>();
            }

            using var doc = JsonDocument.Parse(jsonValue.ToString());

            if (!doc.RootElement.TryGetProperty(arrayKey, out var array))
            {
                return new HashSet<string>();
            }

            return array.EnumerateArray()
                .Select(el => el.GetString() ?? "")
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();
        }
        catch
        {
            return new HashSet<string>();
        }
    }
}
