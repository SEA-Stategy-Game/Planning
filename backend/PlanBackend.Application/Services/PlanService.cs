using PlanBackend.Application.DTOs;
using PlanBackend.Application.Interfaces;
using PlanBackend.Application.Validation;
using PlanBackend.Domain.Models;

namespace PlanBackend.Application.Services;

public class PlanService(IPlanRepository repository, ICoreNotifier notifier, PlanValidator validator)
{
    private readonly IPlanRepository _repository = repository;
    private readonly ICoreNotifier   _notifier   = notifier;
    private readonly PlanValidator   _validator  = validator;

    public async Task<SubmitResult> SubmitPlanAsync(GamePlan plan)
    {
        if (plan.UnitPlans == null || plan.UnitPlans.Count == 0)
            return new SubmitResult { Success = false, Errors = ["unit_plans must not be empty."] };

        var errors = await _validator.ValidateAsync(plan);
        if (errors.Count > 0)
            return new SubmitResult { Success = false, Errors = errors };

        var history    = await _repository.GetGamePlanHistoryAsync(plan.GameId, plan.PlayerId);
        plan.Version   = history.Count == 0 ? 1 : history.Max(h => h.Version) + 1;

        // Compute which units from the previous active plan are absent in the new plan
        var activePlan  = await _repository.GetActiveGamePlanAsync(plan.GameId, plan.PlayerId);
        var newUnitIds  = plan.UnitPlans.Select(u => u.UnitId).ToHashSet();
        var stopUnitIds = activePlan?.UnitPlans
            .Select(u => u.UnitId)
            .Where(id => !newUnitIds.Contains(id))
            .ToList() ?? [];

        await _repository.SaveGamePlanAsync(plan);

        try
        {
            await _notifier.NotifyPlanUpdatedAsync(plan.GameId, plan.PlayerId, [..newUnitIds], stopUnitIds);
        }
        catch (Exception)
        {
            // Core unavailable — plan is saved, notification skipped
        }

        return new SubmitResult
        {
            Success        = true,
            NewVersion     = plan.Version,
            UpdatedUnitIds = plan.UnitPlans.Select(u => u.UnitId).ToList()
        };
    }

    public async Task<List<UnitPlan>> GetUnitPlansAsync(string gameId, string playerId, List<string> unitIds)
    {
        if (string.IsNullOrEmpty(gameId))   throw new ArgumentException("gameId is required.",   nameof(gameId));
        if (string.IsNullOrEmpty(playerId)) throw new ArgumentException("playerId is required.", nameof(playerId));
        if (unitIds == null || unitIds.Count == 0) throw new ArgumentException("unitIds must not be empty.", nameof(unitIds));

        return await _repository.GetUnitPlansAsync(gameId, playerId, unitIds);
    }

    public async Task<List<GamePlanSummary>> GetPlanHistoryAsync(string gameId, string playerId)
        => await _repository.GetGamePlanHistoryAsync(gameId, playerId);

    public async Task<GamePlan?> GetPlanVersionAsync(string gameId, string playerId, int version)
        => await _repository.GetGamePlanByVersionAsync(gameId, playerId, version);
}
