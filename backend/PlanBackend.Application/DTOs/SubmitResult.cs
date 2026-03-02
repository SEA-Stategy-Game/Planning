namespace PlanBackend.Application.DTOs;

public class SubmitResult
{
    public bool Success { get; set; }
    public int NewVersion { get; set; }
    public List<string> UpdatedUnitIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
