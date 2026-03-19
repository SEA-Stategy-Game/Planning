using NSubstitute;
using PlanBackend.Application.DTOs;
using PlanBackend.Application.Interfaces;
using PlanBackend.Application.Services;
using PlanBackend.Application.Validation;
using PlanBackend.Domain.Enums;
using PlanBackend.Domain.Models;

namespace PlanBackend.Tests;

public class PlanServiceTests
{
    private readonly IPlanRepository    _repo        = Substitute.For<IPlanRepository>();
    private readonly ICoreNotifier      _notifier    = Substitute.For<ICoreNotifier>();
    private readonly ISenseQueryClient  _senseClient = Substitute.For<ISenseQueryClient>();
    private readonly PlanService        _sut;

    public PlanServiceTests()
    {
        // Core unavailable by default — entity validation skipped
        _senseClient.GetUnitIdsAsync(Arg.Any<string>())
            .Returns(Task.FromResult<IReadOnlySet<string>>(new HashSet<string>()));
        _senseClient.GetResourceIdsAsync(Arg.Any<string>())
            .Returns(Task.FromResult<IReadOnlySet<string>>(new HashSet<string>()));

        var validator = new PlanValidator(_senseClient);
        _sut = new PlanService(_repo, _notifier, validator);
    }

    // --- SubmitPlanAsync ---

    [Fact]
    public async Task SubmitPlan_EmptyUnitPlans_ReturnsFailure()
    {
        var plan = MakePlan([]);

        var result = await _sut.SubmitPlanAsync(plan);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task SubmitPlan_InvalidActionType_ReturnsFailure()
    {
        _repo.GetGamePlanHistoryAsync("g1", "p1").Returns([]);
        _repo.GetActiveGamePlanAsync("g1", "p1").Returns((GamePlan?)null);

        var plan = MakePlan([new UnitPlan
        {
            UnitId = "u1",
            Steps  = [new PlanStep { StepIndex = 0, StepType = StepType.Action, ActionType = "Fly", Parameters = [] }]
        }]);

        var result = await _sut.SubmitPlanAsync(plan);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Fly"));
    }

    [Fact]
    public async Task SubmitPlan_FirstSubmission_SetsVersionTo1()
    {
        _repo.GetGamePlanHistoryAsync("g1", "p1").Returns([]);
        _repo.GetActiveGamePlanAsync("g1", "p1").Returns((GamePlan?)null);
        var plan = MakePlan([MakeUnit("u1")]);

        var result = await _sut.SubmitPlanAsync(plan);

        Assert.True(result.Success);
        Assert.Equal(1, result.NewVersion);
    }

    [Fact]
    public async Task SubmitPlan_SecondSubmission_IncrementsVersion()
    {
        _repo.GetGamePlanHistoryAsync("g1", "p1").Returns(
        [
            new GamePlanSummary { Version = 1 }
        ]);
        _repo.GetActiveGamePlanAsync("g1", "p1").Returns((GamePlan?)null);
        var plan = MakePlan([MakeUnit("u1")]);

        var result = await _sut.SubmitPlanAsync(plan);

        Assert.Equal(2, result.NewVersion);
    }

    [Fact]
    public async Task SubmitPlan_ValidPlan_ReturnsAllUpdatedUnitIds()
    {
        _repo.GetGamePlanHistoryAsync("g1", "p1").Returns([]);
        _repo.GetActiveGamePlanAsync("g1", "p1").Returns((GamePlan?)null);
        var plan = MakePlan([MakeUnit("u1"), MakeUnit("u2")]);

        var result = await _sut.SubmitPlanAsync(plan);

        Assert.Equal(["u1", "u2"], result.UpdatedUnitIds);
    }

    [Fact]
    public async Task SubmitPlan_NoPreviousActivePlan_StopUnitIdsIsEmpty()
    {
        _repo.GetGamePlanHistoryAsync("g1", "p1").Returns([]);
        _repo.GetActiveGamePlanAsync("g1", "p1").Returns((GamePlan?)null);
        var plan = MakePlan([MakeUnit("u1")]);

        await _sut.SubmitPlanAsync(plan);

        await _notifier.Received(1).NotifyPlanUpdatedAsync(
            "g1", "p1",
            Arg.Any<List<string>>(),
            Arg.Is<List<string>>(l => l.Count == 0));
    }

    [Fact]
    public async Task SubmitPlan_NewPlanHasFewerUnits_NotifiesWithStopUnitIds()
    {
        _repo.GetGamePlanHistoryAsync("g1", "p1").Returns(
        [
            new GamePlanSummary { Version = 1 }
        ]);
        // Previous active plan had u1 and u2
        _repo.GetActiveGamePlanAsync("g1", "p1").Returns(MakePlan([MakeUnit("u1"), MakeUnit("u2")]));

        // New plan only contains u2
        var newPlan = MakePlan([MakeUnit("u2")]);

        await _sut.SubmitPlanAsync(newPlan);

        await _notifier.Received(1).NotifyPlanUpdatedAsync(
            "g1", "p1",
            Arg.Is<List<string>>(l => l.SequenceEqual(new[] { "u2" })),
            Arg.Is<List<string>>(l => l.SequenceEqual(new[] { "u1" })));
    }

    // --- GetUnitPlansAsync ---

    [Fact]
    public async Task GetUnitPlans_NullGameId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetUnitPlansAsync("", "p1", ["u1"]));
    }

    [Fact]
    public async Task GetUnitPlans_NullPlayerId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetUnitPlansAsync("g1", "", ["u1"]));
    }

    [Fact]
    public async Task GetUnitPlans_EmptyUnitIds_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetUnitPlansAsync("g1", "p1", []));
    }

    [Fact]
    public async Task GetUnitPlans_ValidArgs_DelegatesToRepository()
    {
        var expected = new List<UnitPlan> { MakeUnit("u1") };
        _repo.GetUnitPlansAsync("g1", "p1", Arg.Is<List<string>>(l => l.SequenceEqual(new[] { "u1" }))).Returns(expected);

        var result = await _sut.GetUnitPlansAsync("g1", "p1", ["u1"]);

        Assert.Equal(expected, result);
        await _repo.Received(1).GetUnitPlansAsync("g1", "p1", Arg.Is<List<string>>(l => l.SequenceEqual(new[] { "u1" })));
    }

    // --- Helpers ---

    private static GamePlan MakePlan(List<UnitPlan> units) => new()
    {
        GameId    = "g1",
        PlayerId  = "p1",
        UnitPlans = units
    };

    private static UnitPlan MakeUnit(string id) => new()
    {
        UnitId = id,
        Steps  =
        [
            new PlanStep
            {
                StepIndex  = 0,
                StepType   = StepType.Action,
                ActionType = "MoveTo",
                Parameters = new() { ["x"] = "10", ["y"] = "20" }
            }
        ]
    };
}
