using PlanBackend.Application.DTOs;
using PlanBackend.Domain.Models;

namespace PlanBackend.Application.Interfaces;

public interface IPlanRepository
{
    Task SaveGamePlanAsync(GamePlan plan);
    Task<UnitPlan?> GetLatestUnitPlanAsync(string gameId, string playerId, string unitId);
    Task<List<GamePlanSummary>> GetGamePlanHistoryAsync(string gameId, string playerId);
    Task<GamePlan?> GetGamePlanByVersionAsync(string gameId, string playerId, int version);
}
