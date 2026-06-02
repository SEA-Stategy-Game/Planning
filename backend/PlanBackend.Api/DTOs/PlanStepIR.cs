using System.Text.Json.Serialization;

namespace PlanBackend.Api.DTOs;

public class PlanStepIR
{
    [JsonPropertyName("step_index")]
    public int StepIndex { get; set; }

    [JsonPropertyName("step_type")]
    public string StepType { get; set; } = string.Empty;

    [JsonPropertyName("action_type")]
    public string ActionType { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public Dictionary<string, string> Parameters { get; set; } = new();

    [JsonPropertyName("body")]
    public List<PlanStepIR> Body { get; set; } = new();

    [JsonPropertyName("else_body")]
    public List<PlanStepIR> ElseBody { get; set; } = new();
}
