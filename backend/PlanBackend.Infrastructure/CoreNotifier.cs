using PlanBackend.Application.Interfaces;

namespace PlanBackend.Infrastructure;

public class CoreNotifier(HttpClient httpClient) : ICoreNotifier
{
    private readonly HttpClient _httpClient = httpClient;

    public Task NotifyPlanUpdatedAsync(string gameId, string playerId, List<string> unitIds)
        => throw new NotImplementedException();
}
