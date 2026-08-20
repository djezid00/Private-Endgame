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
    [Header("Authored agents (auto-collected from children if left empty)")]
    public TagAgent[] chasers;
    public TagAgent[] runners;

    private static bool paramsLogged = false;

    private int  activeChasers  = 1;
    private int  activeRunners  = 1;
    private bool sizesApplied   = false;
    private bool warnedUnapplied = false;

    /// <summary>
    /// Fallback wiring, mirroring ObstacleManager.Awake(). If an inspector array was not
    /// populated by hand, collect that side's TagAgents from this arena's children.
    /// Without this, an unwired prefab throws NullReferenceException on EVERY episode reset
    /// in a headless run — with 16 arenas that is an unreadable log and a dead training job.
    /// Each side is collected INDEPENDENTLY so a half-wired prefab does not discard the side
    /// that was wired correctly.
    /// </summary>
    private void Awake()
    {
        if (chasers == null || chasers.Length == 0) chasers = CollectByTeam(0);
        if (runners == null || runners.Length == 0) runners = CollectByTeam(1);

        if (chasers.Length == 0 || runners.Length == 0)
            Debug.LogError($"[TeamManager] on '{name}': found {chasers.Length} chasers and " +
                           $"{runners.Length} runners. Both must be non-empty — check that the " +
                           $"TagArena prefab contains agents with teamId 0 and 1.");
    }

    /// <summary>
    /// All TagAgents under this arena with the given teamId. includeInactive is essential:
    /// agents 2..4 on each side are authored inactive by design.
    /// </summary>
    private TagAgent[] CollectByTeam(int wantedTeamId)
    {
        var found = GetComponentsInChildren<TagAgent>(true);
        var list  = new System.Collections.Generic.List<TagAgent>();
        foreach (var a in found)
            if (a.teamId == wantedTeamId) list.Add(a);
        return list.ToArray();
    }

    /// <summary>Chasers active this episode. Only meaningful after ApplyTeamSizes().</summary>
    public int ActiveChasers { get { WarnIfUnapplied(); return activeChasers; } }

    /// <summary>Runners active this episode. Only meaningful after ApplyTeamSizes().</summary>
    public int ActiveRunners { get { WarnIfUnapplied(); return activeRunners; } }

    /// <summary>
    /// The backing defaults are 1/1, which is a PLAUSIBLE operating value rather than an
    /// obviously-broken one — so a consumer that reads before ApplyTeamSizes() has run would
    /// silently pin an 8-hour headless run to 1v1 with no error anywhere. Make that loud.
    /// One-shot so it cannot spam a 10^6-reset log.
    /// </summary>
    private void WarnIfUnapplied()
    {
        if (sizesApplied || warnedUnapplied) return;
        warnedUnapplied = true;
        Debug.LogError($"[TeamManager] on '{name}': team sizes read BEFORE ApplyTeamSizes() ran. " +
                       $"Returning the 1v1 default, which would silently mis-size training. " +
                       $"TagArenaManager.ResetArena() must call ApplyTeamSizes() first.");
    }

    /// <summary>
    /// Reads the env-params and activates the corresponding prefix of each array.
    /// Called by TagArenaManager.ResetArena() BEFORE spawn placement.
    /// </summary>
    public void ApplyTeamSizes()
    {
        // Mathf.Clamp(v, 1, 0) returns 0, not 1 — Unity clamps high after low. Mathf.Max
        // restores the documented "at least 1" contract when an array is empty.
        activeChasers = Mathf.Max(1, Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                            .GetWithDefault("num_chasers", 1f)), 1, chasers.Length));
        activeRunners = Mathf.Max(1, Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                            .GetWithDefault("num_runners", 1f)), 1, runners.Length));
        sizesApplied = true;

        if (!paramsLogged)
        {
            Debug.Log($"[TeamManager] num_chasers={activeChasers}, num_runners={activeRunners}");
            paramsLogged = true;
        }

        for (int i = 0; i < chasers.Length; i++)
            chasers[i].gameObject.SetActive(i < activeChasers);
        for (int i = 0; i < runners.Length; i++)
            runners[i].gameObject.SetActive(i < activeRunners);
    }
}
