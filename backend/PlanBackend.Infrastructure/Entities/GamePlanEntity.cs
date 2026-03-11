namespace PlanBackend.Infrastructure.Entities;

public class GamePlanEntity
{
    public Guid Id { get; set; }
    public string GameId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public int Version { get; set; }
    public string GamePlanJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
