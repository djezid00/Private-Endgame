using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Activates N chasers + M runners per episode from two trainer env-params:
///   num_chasers (default 1)
///   num_runners (default 1)
/// Both default to 1 so every pre-Phase-C config produces byte-identical behaviour.
///
/// Attach to the TagArena prefab root alongside TagArenaManager. The authored agent
/// arrays hold up to 4 of each, all inactive in the prefab; this component activates
/// a prefix of each array. Mirrors ObstacleManager's design deliberately.
/// </summary>
public class TeamManager : MonoBehaviour
{
    [Header("Authored agents (drag all 4 chasers and all 4 runners)")]
    public TagAgent[] chasers;
    public TagAgent[] runners;

    private static bool paramsLogged = false;

    /// <summary>Chasers active this episode. Valid after ApplyTeamSizes().</summary>
    public int ActiveChasers { get; private set; } = 1;

    /// <summary>Runners active this episode. Valid after ApplyTeamSizes().</summary>
    public int ActiveRunners { get; private set; } = 1;

    /// <summary>
    /// Reads the env-params and activates the corresponding prefix of each array.
    /// Called by TagArenaManager.ResetArena() BEFORE spawn placement.
    /// </summary>
    public void ApplyTeamSizes()
    {
        ActiveChasers = Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                            .GetWithDefault("num_chasers", 1f)), 1, chasers.Length);
        ActiveRunners = Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                            .GetWithDefault("num_runners", 1f)), 1, runners.Length);

        if (!paramsLogged)
        {
            Debug.Log($"[TeamManager] num_chasers={ActiveChasers}, num_runners={ActiveRunners}");
            paramsLogged = true;
        }

        for (int i = 0; i < chasers.Length; i++)
            chasers[i].gameObject.SetActive(i < ActiveChasers);
        for (int i = 0; i < runners.Length; i++)
            runners[i].gameObject.SetActive(i < ActiveRunners);
    }
}
