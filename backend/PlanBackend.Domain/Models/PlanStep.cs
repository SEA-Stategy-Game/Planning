using PlanBackend.Domain.Enums;

namespace PlanBackend.Domain.Models;

public class PlanStep
{
    public int StepIndex { get; set; }
    public StepType StepType { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
