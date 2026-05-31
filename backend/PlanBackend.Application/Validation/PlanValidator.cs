using PlanBackend.Application.Interfaces;
using PlanBackend.Domain.Enums;
using PlanBackend.Domain.Models;

namespace PlanBackend.Application.Validation;

public class PlanValidator(ISenseQueryClient senseClient)
{
    private readonly ISenseQueryClient _senseClient = senseClient;

    public async Task<List<string>> ValidateAsync(GamePlan plan)
    {
        var errors = new List<string>();

        // Phase 1: Static validation — action types and required parameters
        foreach (var unitPlan in plan.UnitPlans)
        {
            foreach (var step in unitPlan.Steps)
            {
                if (step.StepType != StepType.Action)
                    continue;

                if (!ActionSpec.SupportedActions.Contains(step.ActionType))
                {
                    errors.Add(
                        $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                        $"unsupported action_type '{step.ActionType}'. " +
                        $"Supported: {string.Join(", ", ActionSpec.SupportedActions)}.");
                    continue;
                }

                if (ActionSpec.RequiredParams.TryGetValue(step.ActionType, out var reqPs))
                {
                    foreach (var param in reqPs)
                    {
                        if (!step.Parameters.TryGetValue(param, out var val) || string.IsNullOrWhiteSpace(val))
                            errors.Add(
                                $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                                $"action '{step.ActionType}' missing required parameter '{param}'.");
                    }
                }

                if (ActionSpec.FloatParams.TryGetValue(step.ActionType, out var floatPs))
                {
                    foreach (var param in floatPs)
                    {
                        if (step.Parameters.TryGetValue(param, out var val) &&
                            !string.IsNullOrWhiteSpace(val) &&
                            !float.TryParse(val, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out _))
                            errors.Add(
                                $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                                $"parameter '{param}' must be a number (got '{val}').");
                    }
                }

                if (ActionSpec.IntParams.TryGetValue(step.ActionType, out var intPs))
                {
                    foreach (var param in intPs)
                    {
                        if (step.Parameters.TryGetValue(param, out var val) &&
                            !string.IsNullOrWhiteSpace(val) &&
                            !int.TryParse(val, out _))
                            errors.Add(
                                $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                                $"parameter '{param}' must be an integer (got '{val}').");
                    }
                }

                if (step.ActionType.Equals("Harvest", StringComparison.OrdinalIgnoreCase))
                {
                    var hasTargetId = step.Parameters.TryGetValue("target_id", out var tid)
                                      && !string.IsNullOrWhiteSpace(tid);
                    var hasResourceType = step.Parameters.TryGetValue("resource_type", out var rt)
                                          && !string.IsNullOrWhiteSpace(rt);

                    if (!hasTargetId && !hasResourceType)
                        errors.Add(
                            $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                            $"Harvest requires either 'target_id' (integer) or 'resource_type' " +
                            $"(one of: {string.Join(", ", ActionSpec.HarvestResourceTypes)}).");
                    else if (hasTargetId && !int.TryParse(tid, out _))
                        errors.Add(
                            $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                            $"parameter 'target_id' must be an integer (got '{tid}').");
                    else if (hasResourceType && !ActionSpec.HarvestResourceTypes.Contains(rt!))
                        errors.Add(
                            $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                            $"'resource_type' must be one of: " +
                            $"{string.Join(", ", ActionSpec.HarvestResourceTypes)} (got '{rt}').");
                }
            }
        }

        if (errors.Count > 0)
            return errors;

        // Phase 2: Entity existence validation — best-effort, skipped if Core is unavailable
        try
        {
            var unitIds     = await _senseClient.GetUnitIdsAsync(plan.GameId);
            var resourceIds = await _senseClient.GetResourceIdsAsync(plan.GameId);

            if (unitIds.Count > 0)
            {
                foreach (var unitPlan in plan.UnitPlans)
                {
                    if (!unitIds.Contains(unitPlan.UnitId))
                        errors.Add(
                            $"unit '{unitPlan.UnitId}' does not exist in the current game state.");
                }
            }

            if (resourceIds.Count > 0)
            {
                foreach (var unitPlan in plan.UnitPlans)
                {
                    foreach (var step in unitPlan.Steps.Where(s =>
                        s.StepType == StepType.Action &&
                        s.ActionType.Equals("Harvest", StringComparison.OrdinalIgnoreCase)))
                    {
                        var targetId = step.Parameters.GetValueOrDefault("target_id", "");
                        if (!string.IsNullOrWhiteSpace(targetId) && !resourceIds.Contains(targetId))
                            errors.Add(
                                $"unit '{unitPlan.UnitId}' step {step.StepIndex}: " +
                                $"resource '{targetId}' does not exist in the current game state.");
                    }
                }
            }
        }
        catch
        {
            // Core unavailable — entity validation skipped, plan accepted
        }

        return errors;
    }
}
