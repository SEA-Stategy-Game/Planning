using System.Text.Json.Serialization;

namespace PlanBackend.Api.DTOs;

public class PlanSubmissionIR
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = string.Empty;

    [JsonPropertyName("game_id")]
    public string GameId { get; set; } = string.Empty;

    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("unit_plans")]
    public List<UnitPlanIR> UnitPlans { get; set; } = new();
}
