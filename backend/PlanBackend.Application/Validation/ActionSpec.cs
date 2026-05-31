namespace PlanBackend.Application.Validation;

public static class ActionSpec
{
    public static readonly IReadOnlyDictionary<string, string[]> RequiredParams =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["MoveTo"]    = ["x", "y"],
            ["Construct"] = ["scene", "x", "y"],
        };

    // Harvest is not in RequiredParams because it uses either target_id or resource_type.
    public static readonly IReadOnlySet<string> HarvestResourceTypes =
        new HashSet<string>(["tree", "stone"], StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<string> SupportedActions =
        new HashSet<string>(RequiredParams.Keys.Append("Harvest"), StringComparer.OrdinalIgnoreCase);

    // Parameters that must parse as float (duration is optional, validated only when present)
    public static readonly IReadOnlyDictionary<string, string[]> FloatParams =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["MoveTo"]    = ["x", "y"],
            ["Construct"] = ["x", "y"],
        };

    public static readonly IReadOnlyDictionary<string, string[]> IntParams =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
        };
}
