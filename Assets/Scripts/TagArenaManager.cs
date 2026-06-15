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

    [Header("Arena Settings")]
    public float arenaRadius = 8f;  // half-size of the square arena
    public float spawnY      = 1f;  // Y height agents are placed at on reset

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

        // --- Place chaser on the LEFT half of the arena ---
        // Random.Range ensures it never spawns exactly on the centre line.
        Vector3 chaserPos = new Vector3(
            Random.Range(-arenaRadius + 1f, -1f),
            spawnY,
            Random.Range(-arenaRadius + 1f,  arenaRadius - 1f)
        );

        // --- Place runner on the RIGHT half of the arena ---
        // Separated side prevents instant-collision on spawn.
        Vector3 runnerPos = new Vector3(
            Random.Range(1f, arenaRadius - 1f),
            spawnY,
            Random.Range(-arenaRadius + 1f, arenaRadius - 1f)
        );

        // --- Safety loop: retry runner position if too close to chaser ---
        // This prevents the collision-on-spawn infinite loop.
        int attempts = 0;
        while (Vector3.Distance(chaserPos, runnerPos) < minSpawnDistance
               && attempts < spawnRetryLimit)
        {
            runnerPos = new Vector3(
                Random.Range(1f, arenaRadius - 1f),
                spawnY,
                Random.Range(-arenaRadius + 1f, arenaRadius - 1f)
            );
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
    // STALEMATE — time ran out, nobody won
    // Runner wins a stalemate (survived), chaser loses.
    // ─────────────────────────────────────────────
    private void TriggerStalemate()
    {
        if (episodeEnded) return;
        episodeEnded = true;

        // Runner survived the full episode — reward it
        runner.AddReward(+1f);
        // Chaser failed to catch runner — penalise it
        chaser.AddReward(-1f);

        // End both episodes simultaneously
        chaser.EndEpisode();
        runner.EndEpisode();
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

        if (tagger.teamId == 0) // ── CHASER caught RUNNER ─────────────────────
        {
            // Chaser reward: base +1 plus a time bonus up to +0.5
            // (catches faster = bigger bonus → chaser learns urgency)
            float timeBonus = (1f - taggerProgress) * 0.5f;
            tagger.AddReward(1f + timeBonus);

            // Runner reward: base -1 but survival softens penalty up to +0.5
            // (survived longer = smaller net penalty → runner learns to dodge)
            float survivalBonus = taggedProgress * 0.5f;
            tagged.AddReward(-1f + survivalBonus);
        }
        else // ── RUNNER somehow triggered the collision (edge case) ──────────
        {
            // Treat as a normal catch regardless of who registered the collision
            tagger.AddReward( 1f);
            tagged.AddReward(-1f);
        }

        // End both agents' episodes at the same time
        tagger.EndEpisode();
        tagged.EndEpisode();
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