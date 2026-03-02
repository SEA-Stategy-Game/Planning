using System.Text.Json.Serialization;

namespace PlanBackend.Api.DTOs;

public class UnitPlanIR
{
    [JsonPropertyName("unit_id")]
    public string UnitId { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<PlanStepIR> Steps { get; set; } = new();
}
