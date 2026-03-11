using Microsoft.AspNetCore.Mvc;
using PlanBackend.Api.DTOs;
using PlanBackend.Application.Services;

namespace PlanBackend.Api.Controllers;

[ApiController]
public class PlanController(PlanService service) : ControllerBase
{
    private readonly PlanService _service = service;

    [HttpPost("/plan")]
    public IActionResult PostPlan([FromBody] PlanSubmissionIR r)
    {
        return Ok();
    }

    [HttpGet("/plan/{gameId}/{playerId}/{unitId}")]
    public IActionResult GetUnitPlan(string gameId, string playerId, string unitId)
    {
        return NotFound();
    }

    [HttpGet("/plan/{gameId}/{playerId}/history")]
    public IActionResult GetPlanHistory(string gameId, string playerId)
    {
        return NotFound();
    }

    [HttpGet("/plan/{gameId}/{playerId}/version/{version:int}")]
    public IActionResult GetPlanVersion(string gameId, string playerId, int version)
    {
        return NotFound();
    }
}
