namespace PlanBackend.Application.Interfaces;

public interface ISenseQueryClient
{
    /// <summary>
    /// Returns the set of unit IDs currently active in the game.
    /// Returns an empty set if Core is unavailable — callers must treat empty as "skip validation".
    /// </summary>
    Task<IReadOnlySet<string>> GetUnitIdsAsync(string gameId);

    /// <summary>
    /// Returns the set of resource IDs currently present in the game.
    /// Returns an empty set if Core is unavailable — callers must treat empty as "skip validation".
    /// </summary>
    Task<IReadOnlySet<string>> GetResourceIdsAsync(string gameId);
}
