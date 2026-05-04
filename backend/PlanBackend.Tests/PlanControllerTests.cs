using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PlanBackend.Api.DTOs;

namespace PlanBackend.Tests;

public class PlanControllerTests : IDisposable
{
    private readonly PlanApiFactory _factory = new();
    private readonly HttpClient _client;

    public PlanControllerTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // --- POST /plan ---

    [Fact]
    public async Task PostPlan_ValidBody_Returns200_WithSubmitResult()
    {
        var response = await _client.PostAsJsonAsync("/plan", ValidSubmission());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal(1, body.GetProperty("newVersion").GetInt32());
    }

    [Fact]
    public async Task PostPlan_EmptyUnitPlans_Returns400()
    {
        var submission = ValidSubmission();
        submission.UnitPlans = [];

        var response = await _client.PostAsJsonAsync("/plan", submission);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostPlan_Twice_SameGamePlayer_IncrementsVersion()
    {
        await _client.PostAsJsonAsync("/plan", ValidSubmission());
        var response = await _client.PostAsJsonAsync("/plan", ValidSubmission());

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.GetProperty("newVersion").GetInt32());
    }

    // --- GET /plan/{gameId}/{playerId}?unitIds= ---

    [Fact]
    public async Task GetUnitPlans_AfterSubmit_Returns200_WithSteps()
    {
        await _client.PostAsJsonAsync("/plan", ValidSubmission());

        var response = await _client.GetAsync("/plan/g1/p1?unitIds=u1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = body.GetProperty("unit_plans")[0];
        Assert.Equal("u1", first.GetProperty("unit_id").GetString());
    }

    [Fact]
    public async Task GetUnitPlans_UnknownUnit_Returns404()
    {
        await _client.PostAsJsonAsync("/plan", ValidSubmission());

        var response = await _client.GetAsync("/plan/g1/p1?unitIds=unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUnitPlans_MissingUnitIdsParam_Returns400()
    {
        var response = await _client.GetAsync("/plan/g1/p1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- GET /plan/{gameId}/{playerId}/history ---

    [Fact]
    public async Task GetHistory_AfterSubmit_Returns200()
    {
        await _client.PostAsJsonAsync("/plan", ValidSubmission());

        var response = await _client.GetAsync("/plan/g1/p1/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetArrayLength());
    }

    [Fact]
    public async Task GetHistory_NoPlans_Returns404()
    {
        var response = await _client.GetAsync("/plan/nobody/nobody/history");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- GET /plan/{gameId}/{playerId}/version/{n} ---

    [Fact]
    public async Task GetPlanVersion_AfterSubmit_Returns200()
    {
        await _client.PostAsJsonAsync("/plan", ValidSubmission());

        var response = await _client.GetAsync("/plan/g1/p1/version/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanVersion_UnknownVersion_Returns404()
    {
        var response = await _client.GetAsync("/plan/g1/p1/version/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Helpers ---

    private static PlanSubmissionIR ValidSubmission() => new()
    {
        SchemaVersion = "1.0",
        GameId = "g1",
        PlayerId = "p1",
        UnitPlans =
        [
            new UnitPlanIR
            {
                UnitId = "u1",
                Steps =
                [
                    new PlanStepIR
                    {
                        StepIndex = 0,
                        StepType = "action",
                        ActionType = "MoveTo",
                        Parameters = new() { ["x"] = "3", ["y"] = "5" }
                    }
                ]
            }
        ]
    };
}

internal sealed class PlanApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"plantest_{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    }

}
