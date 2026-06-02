using System.Text.Json;
using PlanBackend.Application.Interfaces;
using StackExchange.Redis;

namespace PlanBackend.Infrastructure;

public class RedisCoreNotifier(IConnectionMultiplexer redisConnection) : ICoreNotifier
{
    private readonly ISubscriber _subscriber = redisConnection.GetSubscriber();

    public async Task NotifyPlanUpdatedAsync(string gameId, string playerId, List<string> unitIds, List<string> stopUnitIds)
    {
        var payload = new { game_id = gameId, player_id = playerId, unit_ids = unitIds, stop_unit_ids = stopUnitIds };
        var json = JsonSerializer.Serialize(payload);
        
        var channel = $"planning.{gameId}.plan-updated";
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), json);
    }
}
