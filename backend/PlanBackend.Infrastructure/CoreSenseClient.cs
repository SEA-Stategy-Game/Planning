using System.Text.Json;
using PlanBackend.Application.Interfaces;

namespace PlanBackend.Infrastructure;

public class CoreSenseClient(HttpClient httpClient) : ISenseQueryClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IReadOnlySet<string>> GetUnitIdsAsync(string gameId)
        => await FetchIdsAsync(gameId, "units");

    public async Task<IReadOnlySet<string>> GetResourceIdsAsync(string gameId)
        => await FetchIdsAsync(gameId, "resources");

    private async Task<IReadOnlySet<string>> FetchIdsAsync(string gameId, string arrayKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/game-state?gameId={gameId}");
            if (!response.IsSuccessStatusCode)
                return new HashSet<string>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty(arrayKey, out var array))
                return new HashSet<string>();

            return array.EnumerateArray()
                .Select(el => el.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "")
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();
        }
        catch
        {
            return new HashSet<string>();
        }
    }
}
