using Microsoft.AspNetCore.Mvc;
using PlanBackend.Api.DTOs;
using PlanBackend.Application.Services;
using PlanBackend.Domain.Enums;
using PlanBackend.Domain.Models;

namespace PlanBackend.Api.Controllers;

[ApiController]
public class PlanController(PlanService service) : ControllerBase
{
    private readonly PlanService _service = service;

    [HttpPost("/plan")]
    public async Task<IActionResult> PostPlan([FromBody] PlanSubmissionIR r)
    {
        var gamePlan = new GamePlan
        {
            Id = Guid.NewGuid(),
            GameId = r.GameId,
            PlayerId = r.PlayerId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            UnitPlans = r.UnitPlans.Select(u => new UnitPlan
            {
                UnitId = u.UnitId,
                Steps = u.Steps.Select(s => new PlanStep
                {
                    StepIndex = s.StepIndex,
                    StepType = Enum.Parse<StepType>(s.StepType, ignoreCase: true),
                    ActionType = s.ActionType,
                    Parameters = s.Parameters
                }).ToList()
            }).ToList()
        };

        var result = await _service.SubmitPlanAsync(gamePlan);

        if (!result.Success)
            return BadRequest(result.Errors);

        return Ok(result);
    }

    [HttpGet("/plan/{gameId}/{playerId}")]
    public async Task<IActionResult> GetUnitPlans(string gameId, string playerId, [FromQuery] string? unitIds)
    {
        if (string.IsNullOrWhiteSpace(unitIds))
            return BadRequest("unitIds query parameter is required.");

        var ids = unitIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (ids.Count == 0)
            return BadRequest("unitIds must contain at least one unit ID.");

        var unitPlans = await _service.GetUnitPlansAsync(gameId, playerId, ids);

        if (unitPlans.Count == 0)
            return NotFound();

        var response = unitPlans.Select(u => new UnitPlanResponse
        {
            UnitId = u.UnitId,
            Steps = u.Steps.Select(s => new PlanStepIR
            {
                StepIndex = s.StepIndex,
                StepType = s.StepType.ToString().ToLowerInvariant(),
                ActionType = s.ActionType,
                Parameters = s.Parameters
            }).ToList()
        }).ToList();

        return Ok(new UnitPlanCollectionResponse { UnitPlans = response });
    }

    [HttpGet("/plan/{gameId}/{playerId}/history")]
    public async Task<IActionResult> GetPlanHistory(string gameId, string playerId)
    {
        var history = await _service.GetPlanHistoryAsync(gameId, playerId);
        if (history.Count == 0)
            return NotFound();
        return Ok(history);
    }

    [HttpGet("/plan/{gameId}/{playerId}/version/{version:int}")]
    public async Task<IActionResult> GetPlanVersion(string gameId, string playerId, int version)
    {
        var plan = await _service.GetPlanVersionAsync(gameId, playerId, version);
        if (plan == null)
            return NotFound();
        return Ok(plan);
    }
}
