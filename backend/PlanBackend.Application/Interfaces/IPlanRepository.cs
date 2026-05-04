using PlanBackend.Application.DTOs;
using PlanBackend.Domain.Models;

namespace PlanBackend.Application.Interfaces;

public interface IPlanRepository
{
    Task SaveGamePlanAsync(GamePlan plan);
    Task<List<UnitPlan>> GetUnitPlansAsync(string gameId, string playerId, List<string> unitIds);
    Task<List<GamePlanSummary>> GetGamePlanHistoryAsync(string gameId, string playerId);
    Task<GamePlan?> GetGamePlanByVersionAsync(string gameId, string playerId, int version);
    Task<GamePlan?> GetActiveGamePlanAsync(string gameId, string playerId);
}
