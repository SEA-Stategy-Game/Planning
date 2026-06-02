using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlanBackend.Application.DTOs;
using PlanBackend.Application.Interfaces;
using PlanBackend.Domain.Models;
using PlanBackend.Infrastructure.Entities;

namespace PlanBackend.Infrastructure;

public class PlanRepository(PlanDbContext context) : IPlanRepository
{
    private readonly PlanDbContext _context = context;

    public async Task SaveGamePlanAsync(GamePlan plan)
    {
        await _context.GamePlans
            .Where(g => g.GameId == plan.GameId && g.PlayerId == plan.PlayerId && g.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.IsActive, false));

        var entityId = plan.Id == Guid.Empty ? Guid.NewGuid() : plan.Id;
        var json = JsonSerializer.Serialize(plan);

        _context.GamePlans.Add(new GamePlanEntity
        {
            Id = entityId,
            GameId = plan.GameId,
            PlayerId = plan.PlayerId,
            Version = plan.Version,
            GamePlanJson = json,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        foreach (var unitPlan in plan.UnitPlans)
        {
            var unitJson = JsonSerializer.Serialize(unitPlan.Steps);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(unitJson)));
            _context.UnitPlans.Add(new UnitPlanEntity
            {
                Id = Guid.NewGuid(),
                GamePlanId = entityId,
                GameId = plan.GameId,
                PlayerId = plan.PlayerId,
                UnitId = unitPlan.UnitId,
                ContentHash = hash
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<UnitPlan>> GetUnitPlansAsync(string gameId, string playerId, List<string> unitIds)
    {
        var activePlan = await _context.GamePlans
            .FirstOrDefaultAsync(g => g.GameId == gameId && g.PlayerId == playerId && g.IsActive);

        if (activePlan == null)
            return [];

        var gamePlan = JsonSerializer.Deserialize<GamePlan>(activePlan.GamePlanJson);
        if (gamePlan == null)
            return [];

        return gamePlan.UnitPlans
            .Where(u => unitIds.Contains(u.UnitId))
            .ToList();
    }

    public async Task<List<GamePlanSummary>> GetGamePlanHistoryAsync(string gameId, string playerId)
    {
        return await _context.GamePlans
            .Where(g => g.GameId == gameId && g.PlayerId == playerId)
            .OrderByDescending(g => g.Version)
            .Select(g => new GamePlanSummary
            {
                Version = g.Version,
                CreatedAt = g.CreatedAt,
                IsActive = g.IsActive,
                UnitCount = _context.UnitPlans.Count(u => u.GamePlanId == g.Id)
            })
            .ToListAsync();
    }

    public async Task<GamePlan?> GetGamePlanByVersionAsync(string gameId, string playerId, int version)
    {
        var entity = await _context.GamePlans
            .FirstOrDefaultAsync(g => g.GameId == gameId && g.PlayerId == playerId && g.Version == version);

        if (entity == null)
            return null;

        return JsonSerializer.Deserialize<GamePlan>(entity.GamePlanJson);
    }

    public async Task<GamePlan?> GetActiveGamePlanAsync(string gameId, string playerId)
    {
        var entity = await _context.GamePlans
            .Where(g => g.GameId == gameId && g.PlayerId == playerId && g.IsActive)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return JsonSerializer.Deserialize<GamePlan>(entity.GamePlanJson);
    }
}
