using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlanBackend.Domain.Enums;
using PlanBackend.Domain.Models;
using PlanBackend.Infrastructure;

namespace PlanBackend.Tests;

// xUnit creates a new instance per [Fact], so each test gets its own in-memory SQLite database.
public class PlanRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PlanDbContext _ctx;
    private readonly PlanRepository _repo;

    public PlanRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<PlanDbContext>()
            .UseSqlite(_connection)
            .Options;
        _ctx = new PlanDbContext(options);
        _ctx.Database.EnsureCreated();
        _repo = new PlanRepository(_ctx);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _connection.Dispose();
    }

    // --- SaveGamePlanAsync ---

    [Fact]
    public async Task Save_FirstPlan_IsActive()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1));

        var row = await _ctx.GamePlans.SingleAsync();
        Assert.True(row.IsActive);
        Assert.Equal(1, row.Version);
        Assert.Equal("g1", row.GameId);
    }

    [Fact]
    public async Task Save_SecondPlan_DeactivatesFirst()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1));
        await _repo.SaveGamePlanAsync(MakePlan(version: 2));

        var rows = await _ctx.GamePlans.AsNoTracking().OrderBy(g => g.Version).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.False(rows[0].IsActive);
        Assert.True(rows[1].IsActive);
    }

    [Fact]
    public async Task Save_CreatesUnitPlanRows()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1, unitIds: ["u1", "u2"]));

        var unitRows = await _ctx.UnitPlans.ToListAsync();
        Assert.Equal(2, unitRows.Count);
        Assert.Contains(unitRows, u => u.UnitId == "u1");
        Assert.Contains(unitRows, u => u.UnitId == "u2");
    }

    // --- GetUnitPlansAsync ---

    [Fact]
    public async Task GetUnitPlans_ReturnsMatchingUnits()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1, unitIds: ["u1", "u2"]));

        var result = await _repo.GetUnitPlansAsync("g1", "p1", ["u1"]);

        Assert.Single(result);
        Assert.Equal("u1", result[0].UnitId);
    }

    [Fact]
    public async Task GetUnitPlans_UnknownUnit_ReturnsEmpty()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1, unitIds: ["u1"]));

        var result = await _repo.GetUnitPlansAsync("g1", "p1", ["unknown"]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnitPlans_NoActivePlan_ReturnsEmpty()
    {
        var result = await _repo.GetUnitPlansAsync("g1", "p1", ["u1"]);

        Assert.Empty(result);
    }

    // --- GetGamePlanHistoryAsync ---

    [Fact]
    public async Task GetHistory_ReturnsOrderedNewestFirst()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1));
        await _repo.SaveGamePlanAsync(MakePlan(version: 2));

        var history = await _repo.GetGamePlanHistoryAsync("g1", "p1");

        Assert.Equal(2, history.Count);
        Assert.Equal(2, history[0].Version);
        Assert.Equal(1, history[1].Version);
    }

    // --- GetGamePlanByVersionAsync ---

    [Fact]
    public async Task GetByVersion_KnownVersion_ReturnsPlan()
    {
        await _repo.SaveGamePlanAsync(MakePlan(version: 1));

        var plan = await _repo.GetGamePlanByVersionAsync("g1", "p1", 1);

        Assert.NotNull(plan);
        Assert.Equal("g1", plan.GameId);
    }

    [Fact]
    public async Task GetByVersion_UnknownVersion_ReturnsNull()
    {
        var plan = await _repo.GetGamePlanByVersionAsync("g1", "p1", 99);

        Assert.Null(plan);
    }

    // --- Helpers ---

    private static GamePlan MakePlan(int version, string[]? unitIds = null) => new()
    {
        Id = Guid.NewGuid(),
        GameId = "g1",
        PlayerId = "p1",
        Version = version,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UnitPlans = (unitIds ?? ["u1"]).Select(id => new UnitPlan
        {
            UnitId = id,
            Steps = [new PlanStep { StepIndex = 0, StepType = StepType.Action, ActionType = "MoveTo" }]
        }).ToList()
    };
}
