using PlanBackend.Application.DTOs;
using PlanBackend.Application.Interfaces;
using PlanBackend.Domain.Models;
using PlanBackend.Infrastructure.Entities;

namespace PlanBackend.Infrastructure;

public class PlanRepository : IPlanRepository
{
    public Task SaveGamePlanAsync(GamePlan plan)
        => throw new NotImplementedException();

    public Task<UnitPlan?> GetLatestUnitPlanAsync(string gameId, string playerId, string unitId)
        => throw new NotImplementedException();

    public Task<List<GamePlanSummary>> GetGamePlanHistoryAsync(string gameId, string playerId)
        => throw new NotImplementedException();

    public Task<GamePlan?> GetGamePlanByVersionAsync(string gameId, string playerId, int version)
        => throw new NotImplementedException();

    private UnitPlan MapToDomain(UnitPlanEntity entity)
        => throw new NotImplementedException();

    private GamePlanEntity MapToDomainEntity(GamePlan plan)
        => throw new NotImplementedException();
}
