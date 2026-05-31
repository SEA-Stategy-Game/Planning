using Microsoft.Extensions.Configuration;
using PlanBackend.Application.Interfaces;

namespace PlanBackend.Infrastructure.Strategies;

public class CoreNotifierStrategyContext(
    RedisNotifier redisNotifier,
    CoreNotifier httpNotifier,
    IConfiguration configuration) : ICoreNotifier
{
    private readonly RedisNotifier _redisNotifier = redisNotifier;
    private readonly CoreNotifier _httpNotifier = httpNotifier;
    private readonly IConfiguration _configuration = configuration;

    private ICoreNotifier CurrentStrategy
    {
        get
        {
            return _configuration.GetValue<bool>("USE_REDIS") 
                ? _redisNotifier 
                : _httpNotifier;
        }
    }

    public Task NotifyPlanUpdatedAsync(string gameId, string playerId, List<string> newUnitIds, List<string> stopUnitIds)
        => CurrentStrategy.NotifyPlanUpdatedAsync(gameId, playerId, newUnitIds, stopUnitIds);
}