namespace PlanBackend.Infrastructure.Entities;

public class UnitPlanEntity
{
    public Guid Id { get; set; }
    public Guid GamePlanId { get; set; }
    public string GameId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string UnitId { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
}
