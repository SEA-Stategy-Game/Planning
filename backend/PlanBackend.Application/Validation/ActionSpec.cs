namespace PlanBackend.Application.Validation;

public static class ActionSpec
{
    public static readonly IReadOnlyDictionary<string, string[]> RequiredParams =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["MoveTo"]    = ["x", "y"],
            ["Harvest"]   = ["target_id"],
            ["Construct"] = ["scene", "x", "y"],
        };

    public static readonly IReadOnlySet<string> SupportedActions =
        new HashSet<string>(RequiredParams.Keys, StringComparer.OrdinalIgnoreCase);

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
            ["Harvest"] = ["target_id"],
        };
}
