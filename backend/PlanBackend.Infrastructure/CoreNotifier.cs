using System.Text;
using System.Text.Json;
using PlanBackend.Application.Interfaces;

namespace PlanBackend.Infrastructure;

public class CoreNotifier(HttpClient httpClient) : ICoreNotifier
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task NotifyPlanUpdatedAsync(string gameId, string playerId, List<string> unitIds)
    {
        var payload = new { game_id = gameId, player_id = playerId, unit_ids = unitIds };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _httpClient.PostAsync("/plan-updated", content);
    }
}
