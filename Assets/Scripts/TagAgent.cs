using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TagAgent : Agent
{
    // ─────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ─────────────────────────────────────────────
    [Header("Arena Link")]
    public TagArenaManager arena;   // set automatically by TagArenaManager or drag in Inspector

    [Header("Role")]
    public int teamId = 0;          // 0 = chaser, 1 = runner — MUST match BehaviorParameters TeamId

    [Header("Movement")]
    public float moveSpeed  = 5f;   // units/second for forward/back movement
    public float turnSpeed  = 180f; // degrees/second for left/right rotation

    [Header("Reward Shaping (chaser only)")]
    public float arenaDiagonal = 28.28f; // max chaser↔runner planar distance (20x20 floor), normalises Φ
    public float shapingGamma  = 0.99f;  // MUST match trainer extrinsic.gamma

    // Set once per episode from environment_parameters (0 in the sparse arm).
    private float distanceShapingCoef = 0f;
    private float prevPotential       = 0f;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────
    private Rigidbody rb;

    // ─────────────────────────────────────────────
    // INITIALIZE — runs ONCE when the agent is first created
    // Used for one-time setup (caching components).
    // Do NOT put per-episode logic here.
    // ─────────────────────────────────────────────
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ─────────────────────────────────────────────
    // ON EPISODE BEGIN — runs at the START of every episode
    //
    // KEY FIX: Only the CHASER (teamId == 0) calls ResetArena().
    // Previously BOTH agents called ResetArena(), causing a double-reset
    // race condition that spawned agents on top of each other,
    // triggering an instant collision → instant episode end → infinite loop.
    // ─────────────────────────────────────────────
    public override void OnEpisodeBegin()
    {
        if (teamId == 0)
        {
            arena.ResetArena();

            // Select this arm's shaping coefficient from the trainer config
            // (environment_parameters.distance_shaping_coef). 0 ⇒ sparse arm.
            distanceShapingCoef = Academy.Instance.EnvironmentParameters
                .GetWithDefault("distance_shaping_coef", 0f);

            // Seed Φ from the freshly-reset spawn so the first PBS delta is well-defined.
            prevPotential = CurrentPotential();
        }

        // Runner does nothing here — it just waits for chaser's reset to place it.
    }

    // ─────────────────────────────────────────────
    // COLLECT OBSERVATIONS — called every step before the agent acts
    // Total vector observations: 18 floats
    // (Ray Perception Sensor adds its own floats automatically on top)
    // ─────────────────────────────────────────────
    public override void CollectObservations(VectorSensor sensor)
    {
        // ── SELF (9 floats) ──────────────────────────────────────────────────
        // localPosition: where am I in the arena? (3 floats)
        sensor.AddObservation(transform.localPosition);

        // linearVelocity: how fast and in what direction am I moving? (3 floats)
        sensor.AddObservation(rb.linearVelocity);

        // forward: which direction am I facing? (3 floats)
        sensor.AddObservation(transform.forward);

        // ── OPPONENT (9 floats) ──────────────────────────────────────────────
        TagAgent   opponent   = arena.GetOpponent(this);
        Rigidbody  opponentRb = opponent.GetComponent<Rigidbody>();

        // Relative position: where is the opponent relative to ME? (3 floats)
        // Using relative (not absolute) position makes the observation
        // arena-position-independent — the agent learns "opponent is 3m right"
        // rather than "opponent is at world position (7, 1, 2)".
        sensor.AddObservation(opponent.transform.localPosition - transform.localPosition);

        // Opponent's velocity: how fast is the opponent moving and where? (3 floats)
        sensor.AddObservation(opponentRb.linearVelocity);

        // Opponent's facing direction: which way is the opponent turned? (3 floats)
        sensor.AddObservation(opponent.transform.forward);

        // TOTAL: 18 floats — must match VectorObservationSize in BehaviorParameters
    }

    // Current potential Φ(s) for the chaser, from live positions. Uses localPosition
    // to match the observation frame (both agents are children of the same arena).
    private float CurrentPotential()
    {
        TagAgent opponent = arena.GetOpponent(this);
        return TagReward.Potential(transform.localPosition,
                                   opponent.transform.localPosition,
                                   distanceShapingCoef, arenaDiagonal);
    }

    // ─────────────────────────────────────────────
    // ON ACTION RECEIVED — called every FixedUpdate step when the trainer sends actions
    // actions.ContinuousActions[0] = move  (-1 = back, 0 = stop, +1 = forward)
    // actions.ContinuousActions[1] = turn  (-1 = left, 0 = straight, +1 = right)
    // ─────────────────────────────────────────────
    public override void OnActionReceived(ActionBuffers actions)
    {
        // ── READ ACTIONS ─────────────────────────────────────────────────────
        // Clamp ensures the network output never exceeds [-1, 1]
        float move = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float turn = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // ── APPLY MOVEMENT ───────────────────────────────────────────────────
        // MovePosition moves the Rigidbody kinematically, respecting physics colliders.
        // We multiply by Time.fixedDeltaTime to keep speed frame-rate-independent.
        rb.MovePosition(transform.position
            + transform.forward * move * moveSpeed * Time.fixedDeltaTime);

        // Rotate on the Y axis only (yaw) — no tilting or rolling
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.fixedDeltaTime);

        // ── ROLE-BASED STEP REWARD ───────────────────────────────────────────
        if (teamId == 0) // CHASER
        {
            // Small negative reward every step → chaser is punished for wasting time
            // This creates urgency: catch the runner as fast as possible.
            AddReward(-0.001f);

            // Potential-based shaping: reward closing distance to the runner.
            // Policy-invariant (Ng et al. 1999). No-op in the sparse arm (coef 0 ⇒ Φ ≡ 0).
            float curPotential = CurrentPotential();
            AddReward(TagReward.ShapingDelta(prevPotential, curPotential, shapingGamma));
            prevPotential = curPotential;

            // Only the chaser ticks the arena step clock.
            // If the runner also called arena.Step(), the timer would advance
            // twice per frame (double-speed stalemate).
            arena.Step();
        }
        else // RUNNER
        {
            // Small positive reward every step → runner is rewarded for surviving
            // This creates evasion: stay alive as long as possible.
            AddReward(+0.001f);
        }
    }

    // ─────────────────────────────────────────────
    // ON COLLISION ENTER — fires when this agent physically touches another collider
    //
    // We use OnCollisionEnter (not OnTriggerEnter) because the BoxCollider
    // is NOT set to IsTrigger — it's a solid physics collider.
    // ─────────────────────────────────────────────
    private void OnCollisionEnter(Collision collision)
    {
        // Only react to collisions with objects tagged "Agent"
        // (avoids reacting to wall bounces or floor contacts)
        if (!collision.collider.CompareTag("Agent")) return;

        TagAgent other = collision.collider.GetComponent<TagAgent>();

        // Null guard: if the other object has no TagAgent, ignore it
        if (other == null) return;

        // Notify the arena manager — it decides who wins based on teamId
        arena.OnAgentTagged(this, other);
    }

    // ─────────────────────────────────────────────
    // HEURISTIC — manual keyboard control for testing WITHOUT the ML trainer
    // Press Play in Unity WITHOUT running mlagents-learn to test movement.
    // W/S = move forward/back, A/D = turn left/right
    // ─────────────────────────────────────────────
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;

        c[0] = UnityEngine.InputSystem.Keyboard.current.wKey.isPressed ?  1f :
               UnityEngine.InputSystem.Keyboard.current.sKey.isPressed ? -1f : 0f;

        c[1] = UnityEngine.InputSystem.Keyboard.current.dKey.isPressed ?  1f :
               UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ? -1f : 0f;
    }
}