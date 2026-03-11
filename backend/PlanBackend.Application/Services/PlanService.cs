using PlanBackend.Application.DTOs;
using PlanBackend.Application.Interfaces;
using PlanBackend.Domain.Models;

namespace PlanBackend.Application.Services;

public class PlanService(IPlanRepository repository, ICoreNotifier notifier)
{
    private readonly IPlanRepository _repository = repository;
    private readonly ICoreNotifier _notifier = notifier;

    public Task<SubmitResult> SubmitPlanAsync(GamePlan plan)
        => throw new NotImplementedException();

    public Task<UnitPlan?> GetUnitPlanAsync(string gameId, string playerId, string unitId)
        => throw new NotImplementedException();

    public Task<List<GamePlanSummary>> GetPlanHistoryAsync(string gameId, string playerId)
        => throw new NotImplementedException();

    public Task<GamePlan?> GetPlanVersionAsync(string gameId, string playerId, int version)
        => throw new NotImplementedException();
}
