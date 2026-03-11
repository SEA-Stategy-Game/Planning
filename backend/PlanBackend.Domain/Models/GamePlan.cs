namespace PlanBackend.Domain.Models;

public class GamePlan
{
    public Guid Id { get; set; }
    public string GameId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UnitPlan> UnitPlans { get; set; } = new();
}
