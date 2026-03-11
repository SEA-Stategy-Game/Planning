namespace PlanBackend.Domain.Models;

public class UnitPlan
{
    public string UnitId { get; set; } = string.Empty;
    public List<PlanStep> Steps { get; set; } = new();
}
