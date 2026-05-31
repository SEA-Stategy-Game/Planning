using Microsoft.Extensions.Configuration;
using PlanBackend.Application.Interfaces;

namespace PlanBackend.Infrastructure.Strategies;

public class SenseQueryStrategyContext(
    RedisSenseClient redisClient,
    CoreSenseClient httpClient,
    IConfiguration configuration) : ISenseQueryClient
{
    private readonly RedisSenseClient _redisClient = redisClient;
    private readonly CoreSenseClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;

    private ISenseQueryClient CurrentStrategy
    {
        get
        {
            return _configuration.GetValue<bool>("USE_REDIS") 
                ? _redisClient 
                : _httpClient;
        }
    }

    public Task<IReadOnlySet<string>> GetUnitIdsAsync(string gameId) 
        => CurrentStrategy.GetUnitIdsAsync(gameId);

    public Task<IReadOnlySet<string>> GetResourceIdsAsync(string gameId) 
        => CurrentStrategy.GetResourceIdsAsync(gameId);
}