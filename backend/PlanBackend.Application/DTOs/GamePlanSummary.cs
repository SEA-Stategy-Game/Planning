namespace PlanBackend.Application.DTOs;

public class GamePlanSummary
{
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int UnitCount { get; set; }
}
