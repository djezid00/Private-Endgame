using UnityEngine;
using Unity.MLAgents;

public class TagArenaManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR REFERENCES
    // ─────────────────────────────────────────────
    [Header("Agent References")]
    public TagAgent chaser;   // drag ChaserAgent here in Inspector
    public TagAgent runner;   // drag RunnerAgent here in Inspector

    [Header("Obstacles (optional — leave empty for the legacy open arena)")]
    public ObstacleManager obstacles;   // drag the TagArena prefab's Obstacles object here

    [Header("Arena Settings")]
    public float arenaRadius = 8f;    // half-size of the square arena
    public float spawnY      = 0.5f;  // Y height agents are placed at on reset.
                                      // The agent is a 1x1x1 box (centre = 0.5 above its base),
                                      // so 0.5 rests it flush on a floor whose top is at y=0
                                      // (previously 1f, which left agents floating ~0.5u at spawn).

    [Header("Spawn Safety")]
    public float minSpawnDistance = 3f; // minimum distance between agents at spawn
    public int   spawnRetryLimit  = 30; // max attempts to find a valid spawn pair

    [Header("Stalemate Prevention")]
    public int maxEpisodeSteps = 2000;  // steps before episode is forced to end

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────
    private int  stepCount    = 0;
    private bool episodeEnded = false;

    private Rigidbody chaserRb;
    private Rigidbody runnerRb;

    // ML-Agents stats sink — surfaces custom scalars in TensorBoard.
    private StatsRecorder stats;

    // ─────────────────────────────────────────────
    // MA-POCA TEAM GROUPS
    // Each role is its own cooperative group. With 1v1 each group holds a single
    // agent, but routing terminal rewards / episode ends through the group is what
    // makes this a genuine MA-POCA (poca) setup rather than de-facto PPO — and it
    // means scaling to multiple chasers later is just extra RegisterAgent() calls.
    // All agents in a group MUST share the same Behavior Name (Chaser / Runner).
    // ─────────────────────────────────────────────
    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    // ─────────────────────────────────────────────
    // UNITY START — cache Rigidbody references once
    // ─────────────────────────────────────────────
    // Called once when the scene starts.
    // We cache the Rigidbody components here instead of calling
    // GetComponent<>() every frame, which is expensive.
    private void Start()
    {
        chaserRb = chaser.GetComponent<Rigidbody>();
        runnerRb = runner.GetComponent<Rigidbody>();

        // Build the two role groups and register their agent(s).
        // To add a second chaser later: chaserGroup.RegisterAgent(secondChaser);
        chaserGroup = new SimpleMultiAgentGroup();
        chaserGroup.RegisterAgent(chaser);

        runnerGroup = new SimpleMultiAgentGroup();
        runnerGroup.RegisterAgent(runner);

        // StatsRecorder lets us log episode outcomes as TensorBoard scalars.
        stats = Academy.Instance.StatsRecorder;
    }

    // ─────────────────────────────────────────────
    // RESET ARENA
    // Called ONLY by the chaser's OnEpisodeBegin().
    // Runner does NOT call this — that was the double-reset bug.
    // ─────────────────────────────────────────────
    public void ResetArena()
    {
        // Reset episode state flags
        episodeEnded = false;
        stepCount    = 0;

        // Obstacles FIRST, agents second — agent spawn rejection needs the new positions.
        if (obstacles != null) obstacles.ResetObstacles();

        // --- Place chaser on the LEFT half, runner on the RIGHT half ---
        // Separated sides prevent instant-collision on spawn; SampleSpawn keeps
        // both out of the obstacle clearance zone.
        Vector3 chaserPos = SampleSpawn(-arenaRadius + 1f, -1f);
        Vector3 runnerPos = SampleSpawn(1f, arenaRadius - 1f);

        // --- Safety loop: retry runner position if too close to chaser ---
        int attempts = 0;
        while (Vector3.Distance(chaserPos, runnerPos) < minSpawnDistance
               && attempts < spawnRetryLimit)
        {
            runnerPos = SampleSpawn(1f, arenaRadius - 1f);
            attempts++;
        }

        // --- Apply positions and random rotations ---
        chaser.transform.localPosition = chaserPos;
        runner.transform.localPosition = runnerPos;

        chaser.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        runner.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // --- Zero out all physics velocity ---
        // Without this, residual velocity from the previous episode carries over.
        chaserRb.linearVelocity  = Vector3.zero;
        chaserRb.angularVelocity = Vector3.zero;
        runnerRb.linearVelocity  = Vector3.zero;
        runnerRb.angularVelocity = Vector3.zero;
    }

    // Samples a spawn in [xMin,xMax] x [-arenaRadius+1, arenaRadius-1], re-rolling while the
    // candidate sits inside an active obstacle's clearance. Bounded retries; after the budget
    // it returns the last candidate rather than loop forever (an occasional pillar-adjacent
    // spawn is harmless — physics pushes the box out).
    private Vector3 SampleSpawn(float xMin, float xMax)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < spawnRetryLimit; i++)
        {
            pos = new Vector3(
                Random.Range(xMin, xMax),
                spawnY,
                Random.Range(-arenaRadius + 1f, arenaRadius - 1f));
            if (obstacles == null || obstacles.IsClearOfActiveObstacles(pos)) return pos;
        }
        return pos;
    }

    // ─────────────────────────────────────────────
    // STEP CLOCK
    // Called every FixedUpdate by the chaser ONLY (not the runner).
    // This drives the stalemate timer without double-counting.
    // ─────────────────────────────────────────────
    public void Step()
    {
        if (episodeEnded) return; // ignore calls after episode already finished

        stepCount++;

        if (stepCount >= maxEpisodeSteps)
            TriggerStalemate();
    }

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

    // ─────────────────────────────────────────────
    // GET OPPONENT
    // Utility used by TagAgent.CollectObservations()
    // to retrieve the reference to the OTHER agent.
    // ─────────────────────────────────────────────
    public TagAgent GetOpponent(TagAgent agent)
    {
        return (agent == chaser) ? runner : chaser;
    }
}