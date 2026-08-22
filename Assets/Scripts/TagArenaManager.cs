using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class TagArenaManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR REFERENCES
    // ─────────────────────────────────────────────
    [Header("Team")]
    public TeamManager teams;   // drag the TagArena prefab's TeamManager here

    [Header("Obstacles (optional — leave empty for the legacy open arena)")]
    public ObstacleManager obstacles;

    [Header("Arena Settings")]
    public float arenaRadius = 8f;
    public float spawnY      = 0.5f;

    [Header("Spawn Safety")]
    public float minSpawnDistance = 3f;
    public int   spawnRetryLimit  = 30;

    [Header("Stalemate Prevention")]
    public int maxEpisodeSteps = 2000;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────
    private int  stepCount    = 0;
    private bool episodeEnded = false;

    private readonly List<TagAgent> activeChasers = new List<TagAgent>();
    private readonly List<TagAgent> activeRunners = new List<TagAgent>();
    private readonly List<Vector2>  spawnBuffer   = new List<Vector2>();

    // Reused by AllActiveAgents() — see the note there. Never returned to a retaining caller.
    private readonly List<TagAgent> activeAgentsBuffer = new List<TagAgent>();

    private int runnersCaughtThisEpisode = 0;

    private StatsRecorder stats;
    private System.Random spawnRng;

    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    private void Start()
    {
        // Seeded from UnityEngine.Random, which ML-Agents seeds from --seed, so spawns
        // are reproducible per seed. Same pattern as ObstacleManager.
        spawnRng = new System.Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        stats = Academy.Instance.StatsRecorder;

        // Reset via the Academy hook, NOT a direct call. Environment parameters are not
        // guaranteed to have arrived from the trainer by Start(); reading num_chasers too
        // early would silently run the first episode at the 1v1 default AND latch the
        // wrong values into TeamManager's one-shot log line, breaking smoke criterion 1.
        Academy.Instance.OnEnvironmentReset += ResetArena;
    }

    // ─────────────────────────────────────────────
    // STEP CLOCK — owned by the arena, ticked once per physics step.
    // Previously the chaser called arena.Step() from OnActionReceived, which does not
    // generalize (N chasers would tick N times) and coupled the clock to agent code.
    // ─────────────────────────────────────────────
    private void FixedUpdate()
    {
        if (episodeEnded) return;

        stepCount++;
        if (stepCount >= maxEpisodeSteps)
            TriggerStalemate();
    }

    /// <summary>
    /// Full arena reset. Called from Start() and after every episode end — by the arena
    /// itself, never from an agent's OnEpisodeBegin (that was the old ordering bug).
    /// </summary>
    public void ResetArena()
    {
        episodeEnded = false;
        stepCount    = 0;
        runnersCaughtThisEpisode = 0;

        // 1. Obstacles first — spawn rejection needs their new positions.
        if (obstacles != null) obstacles.ResetObstacles();

        // 2. Team sizes from env-params, activating the agents.
        teams.ApplyTeamSizes();

        // 3. Rebuild the active lists from what TeamManager just activated.
        activeChasers.Clear();
        activeRunners.Clear();
        for (int i = 0; i < teams.ActiveChasers; i++) activeChasers.Add(teams.chasers[i]);
        for (int i = 0; i < teams.ActiveRunners; i++) activeRunners.Add(teams.runners[i]);

        // 4. Sample spawns for everyone at once.
        bool ok = SpawnPlacement.TrySampleSpawns(
            activeChasers.Count, activeRunners.Count, arenaRadius, minSpawnDistance,
            ObstaclePositions(), ObstaclePositionCount(), ObstacleClearance(),
            spawnRng, spawnBuffer);

        if (!ok)
        {
            // Fallback: relax separation rather than break training. Logged so an
            // over-crowded composition is visible in the player log instead of silent.
            Debug.LogWarning($"[TagArenaManager] spawn sampling failed for " +
                             $"{activeChasers.Count}v{activeRunners.Count}; retrying with half separation.");
            ok = SpawnPlacement.TrySampleSpawns(
                activeChasers.Count, activeRunners.Count, arenaRadius, minSpawnDistance * 0.5f,
                ObstaclePositions(), ObstaclePositionCount(), ObstacleClearance(),
                spawnRng, spawnBuffer);
        }

        if (!ok)
        {
            // SpawnPlacement CLEARS its result list on failure, so spawnBuffer is now empty.
            // Indexing it below would throw ArgumentOutOfRangeException and kill the training
            // process mid-run. Fall back to a deterministic grid instead: correctness of the
            // episode matters less than never crashing a 5M-step unattended run.
            Debug.LogError($"[TagArenaManager] spawn sampling failed TWICE for " +
                           $"{activeChasers.Count}v{activeRunners.Count} — using fallback grid. " +
                           $"This composition is over-crowded; lower it.");
            FallbackGridSpawns(activeChasers.Count, activeRunners.Count, spawnBuffer);
        }

        // 5. Place agents, zero physics, randomize yaw.
        for (int i = 0; i < activeChasers.Count; i++)
            PlaceAgent(activeChasers[i], spawnBuffer[i]);
        for (int i = 0; i < activeRunners.Count; i++)
            PlaceAgent(activeRunners[i], spawnBuffer[activeChasers.Count + i]);

        Physics.SyncTransforms();

        // 6. RE-REGISTER every active agent. SetActive(false) auto-unregistered the ones
        //    caught last episode (SimpleMultiAgentGroup subscribes OnAgentDisabled), so
        //    without this the groups silently drain to empty — with no error raised.
        //    RegisterAgent is idempotent, so re-registering survivors is harmless.
        foreach (TagAgent c in activeChasers) chaserGroup.RegisterAgent(c);
        foreach (TagAgent r in activeRunners) runnerGroup.RegisterAgent(r);

        // 7. Seed per-episode shaping state now that positions are final.
        foreach (TagAgent c in activeChasers) c.OnArenaReset();
        foreach (TagAgent r in activeRunners) r.OnArenaReset();
    }

    /// <summary>
    /// Last-resort deterministic spawn layout, used only when rejection sampling has failed
    /// twice. Evenly spaces each team down its own side of the arena. May violate
    /// minSpawnDistance for very large teams — that is preferable to crashing the run.
    /// </summary>
    private void FallbackGridSpawns(int chaserCount, int runnerCount, List<Vector2> buffer)
    {
        buffer.Clear();
        float edge = arenaRadius - 1f;
        for (int i = 0; i < chaserCount; i++)
        {
            float t = chaserCount == 1 ? 0.5f : (float)i / (chaserCount - 1);
            buffer.Add(new Vector2(-edge * 0.5f, Mathf.Lerp(-edge, edge, t)));
        }
        for (int i = 0; i < runnerCount; i++)
        {
            float t = runnerCount == 1 ? 0.5f : (float)i / (runnerCount - 1);
            buffer.Add(new Vector2(edge * 0.5f, Mathf.Lerp(-edge, edge, t)));
        }
    }

    private void PlaceAgent(TagAgent agent, Vector2 xz)
    {
        agent.transform.localPosition = new Vector3(xz.x, spawnY, xz.y);
        agent.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        var arb = agent.GetComponent<Rigidbody>();
        arb.linearVelocity  = Vector3.zero;
        arb.angularVelocity = Vector3.zero;
    }

    private static readonly List<Vector2> emptyObstacles = new List<Vector2>();
    private IReadOnlyList<Vector2> ObstaclePositions() =>
        obstacles != null ? obstacles.ActivePositions : emptyObstacles;
    private int ObstaclePositionCount() =>
        obstacles != null ? obstacles.ActivePositions.Count : 0;
    private float ObstacleClearance() =>
        obstacles != null ? obstacles.agentClearance : 0f;

    // ─────────────────────────────────────────────
    // PPO SUPPORT — individual terminal reward toggle
    // PPO ignores group rewards (AddGroupReward / EndGroupEpisode are POCA-only), so a PPO run would
    // train with no win/lose signal. When the config sets individual_terminal_reward > 0 we ALSO
    // deliver the terminal ±1 (plus the same time/survival bonuses) through each agent's individual
    // AddReward. At group-size-1 this is exactly equivalent to the group reward, so POCA and PPO see
    // the same signal and the comparison stays fair. Defaults to 0 ⇒ the POCA path is byte-identical.
    // ─────────────────────────────────────────────
    private bool IndividualTerminalRewardOn()
        => Academy.Instance.EnvironmentParameters.GetWithDefault("individual_terminal_reward", 0f) > 0.5f;

    // ─────────────────────────────────────────────
    // STALEMATE — time ran out, nobody won
    // Runner wins a stalemate (survived), chaser loses.
    // ─────────────────────────────────────────────
    private void TriggerStalemate()
    {
        if (episodeEnded) return;
        episodeEnded = true;

        // Runner group survived the full episode — reward it
        runnerGroup.AddGroupReward(+1f);
        // Chaser group failed to catch the runner — penalise it
        chaserGroup.AddGroupReward(-1f);

        // PPO also needs the win/lose signal individually (see IndividualTerminalRewardOn).
        if (IndividualTerminalRewardOn())
        {
            runner.AddReward(+1f);
            chaser.AddReward(-1f);
        }

        // Outcome metric — recorded BEFORE ending the episode (the group-end call synchronously
        // resets the arena; see OnAgentTagged). 0 = no catch this episode (averaged ⇒ catch rate).
        stats.Add("Environment/Catch", 0f);

        // Timeout is a TRUNCATION, not a true terminal state, so use
        // GroupEpisodeInterrupted: it bootstraps the value estimate at the cutoff
        // instead of treating it as a real end (correct for stalemate).
        chaserGroup.GroupEpisodeInterrupted();
        runnerGroup.GroupEpisodeInterrupted();
    }

    // ─────────────────────────────────────────────
    // TAGGING EVENT
    // Called from TagAgent.OnCollisionEnter when two agents collide.
    // tagger  = the agent whose OnCollisionEnter fired
    // tagged  = the OTHER agent that was hit
    // ─────────────────────────────────────────────
    public void OnAgentTagged(TagAgent tagger, TagAgent tagged)
    {
        // Guard: ignore if episode already ended (e.g. stalemate fired first)
        if (episodeEnded) return;
        episodeEnded = true;

        // Use the arena's maxEpisodeSteps as fallback if agent MaxStep is 0
        int taggerMax = (tagger.MaxStep > 0) ? tagger.MaxStep : maxEpisodeSteps;
        int taggedMax = (tagged.MaxStep  > 0) ? tagged.MaxStep  : maxEpisodeSteps;

        float taggerProgress = Mathf.Clamp01((float)tagger.StepCount / taggerMax);
        float taggedProgress = Mathf.Clamp01((float)tagged.StepCount / taggedMax);

        bool mirror = IndividualTerminalRewardOn();

        if (tagger.teamId == 0) // ── CHASER caught RUNNER ─────────────────────
        {
            // Chaser group reward: base +1 plus a time bonus up to +0.5
            // (catches faster = bigger bonus → chaser learns urgency)
            float timeBonus = (1f - taggerProgress) * 0.5f;
            // Runner group reward: base -1 but survival softens penalty up to +0.5
            // (survived longer = smaller net penalty → runner learns to dodge)
            float survivalBonus = taggedProgress * 0.5f;

            chaserGroup.AddGroupReward(1f + timeBonus);
            runnerGroup.AddGroupReward(-1f + survivalBonus);

            if (mirror)
            {
                chaser.AddReward(1f + timeBonus);
                runner.AddReward(-1f + survivalBonus);
            }
        }
        else // ── RUNNER somehow triggered the collision (edge case) ──────────
        {
            // A catch is a catch: chaser side wins regardless of which collider fired.
            chaserGroup.AddGroupReward( 1f);
            runnerGroup.AddGroupReward(-1f);

            if (mirror)
            {
                chaser.AddReward( 1f);
                runner.AddReward(-1f);
            }
        }

        // Outcome metrics — recorded BEFORE ending the episode. EndGroupEpisode() synchronously
        // runs the chaser's OnEpisodeBegin → ResetArena, which zeroes stepCount; reading it after
        // that was the bug that logged TimeToCatch = 0. stepCount here is the catch time in
        // physics steps (1 = catch; averaged ⇒ catch rate / mean steps-to-catch).
        stats.Add("Environment/Catch", 1f);
        stats.Add("Environment/TimeToCatch", stepCount);

        // A catch IS a true terminal state → EndGroupEpisode (no value bootstrap).
        chaserGroup.EndGroupEpisode();
        runnerGroup.EndGroupEpisode();
    }

    /// <summary>Nearest ACTIVE opponent to the given agent, or null if none remain.</summary>
    public TagAgent GetNearestOpponent(TagAgent agent)
    {
        List<TagAgent> opponents = (agent.teamId == 0) ? activeRunners : activeChasers;
        TagAgent best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < opponents.Count; i++)
        {
            if (!opponents[i].gameObject.activeInHierarchy) continue;
            float d = (opponents[i].transform.localPosition - agent.transform.localPosition).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = opponents[i]; }
        }
        return best;
    }

    /// <summary>
    /// Every currently-active agent in this arena, both teams.
    ///
    /// Returns a REUSED buffer — consume it immediately, never retain it. Deliberately NOT a
    /// `yield return` iterator: a compiler-generated iterator allocates an enumerator on every
    /// call, and this is called once per agent per decision. At 8 agents x 16 arenas x ~10
    /// decisions/sec that is ~1000 enumerator allocations/sec landing inside env_step, which
    /// Theory §5 measured as 54.5% of production wall-clock. Safe because every caller consumes
    /// the buffer synchronously within its own CollectObservations, single-threaded, no nesting.
    /// </summary>
    public List<TagAgent> AllActiveAgents()
    {
        activeAgentsBuffer.Clear();
        for (int i = 0; i < activeChasers.Count; i++)
            if (activeChasers[i].gameObject.activeInHierarchy) activeAgentsBuffer.Add(activeChasers[i]);
        for (int i = 0; i < activeRunners.Count; i++)
            if (activeRunners[i].gameObject.activeInHierarchy) activeAgentsBuffer.Add(activeRunners[i]);
        return activeAgentsBuffer;
    }
}