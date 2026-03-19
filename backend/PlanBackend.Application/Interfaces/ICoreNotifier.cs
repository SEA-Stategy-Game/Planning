namespace PlanBackend.Application.Interfaces;

public interface ICoreNotifier
{
    Task NotifyPlanUpdatedAsync(string gameId, string playerId, List<string> unitIds, List<string> stopUnitIds);
}
