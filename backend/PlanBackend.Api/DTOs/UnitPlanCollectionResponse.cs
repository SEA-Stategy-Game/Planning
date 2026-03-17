using System.Text.Json.Serialization;

namespace PlanBackend.Api.DTOs;

public class UnitPlanCollectionResponse
{
    [JsonPropertyName("unit_plans")]
    public List<UnitPlanResponse> UnitPlans { get; set; } = new();
}
