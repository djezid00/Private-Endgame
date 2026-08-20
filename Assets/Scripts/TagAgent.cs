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
    public float shapingGamma  = 0.99f;  // fallback only — overridden per episode by env-param "shaping_gamma"

    // Set once per episode from environment_parameters (0 in the sparse arm).
    private float distanceShapingCoef = 0f;
    private float prevPotential       = 0f;

    // One-time log so smoke runs can verify which shaping params the binary received.
    private static bool shapingParamsLogged = false;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────
    private Rigidbody rb;

    /// <summary>
    /// This agent's Rigidbody, cached. Other agents read it every decision step when
    /// building their BufferSensor entities; GetComponent there would cost tens of
    /// thousands of native lookups per second at 8 agents x 16 arenas. Lazily resolved
    /// because a teammate may be observed before its own Initialize() has run.
    /// </summary>
    public Rigidbody Body
    {
        get
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            return rb;
        }
    }

    private BufferSensorComponent entitySensor;

    // Reused per-entity observation buffer — see the note in CollectObservations.
    private readonly float[] entityScratch = new float[10];

    // ─────────────────────────────────────────────
    // INITIALIZE — runs ONCE when the agent is first created
    // Used for one-time setup (caching components).
    // Do NOT put per-episode logic here.
    // ─────────────────────────────────────────────
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        entitySensor = GetComponent<BufferSensorComponent>();
    }

    // ─────────────────────────────────────────────
    // ON EPISODE BEGIN
    // Arena reset and the step clock are owned by TagArenaManager (Phase C).
    // Agents no longer drive either — with N chasers there is no privileged one,
    // and this also fixes the old ordering bug where the runner was repositioned
    // before its own group episode ended.
    // ─────────────────────────────────────────────
    public override void OnEpisodeBegin() { }

    // ─────────────────────────────────────────────
    // COLLECT OBSERVATIONS — called every step before the agent acts
    // Total vector observations: 18 floats
    // (Ray Perception Sensor adds its own floats automatically on top)
    // ─────────────────────────────────────────────
    public override void CollectObservations(VectorSensor sensor)
    {
        // ── SELF (9 floats) ──────────────────────────────────────────────────
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        sensor.AddObservation(transform.forward);

        // ── NEAREST ACTIVE OPPONENT (9 floats) ───────────────────────────────
        // Kept in the vector observation so that at 1v1 the agent sees byte-identical
        // values to every pre-Phase-C experiment (the buffer below is empty there).
        TagAgent opponent = arena.GetNearestOpponent(this);
        if (opponent != null)
        {
            sensor.AddObservation(opponent.transform.localPosition - transform.localPosition);
            sensor.AddObservation(opponent.Body.linearVelocity);
            sensor.AddObservation(opponent.transform.forward);
        }
        else
        {
            // All opponents caught; episode is about to end. Zeros keep the size fixed.
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
        }
        // TOTAL vector: 18 floats — unchanged from 1v1.

        // ── ALL OTHER ACTIVE AGENTS (BufferSensor, 10 floats each) ───────────
        // entityScratch is reused across every iteration below rather than allocating a
        // fresh float[10] per entity: BufferSensor.AppendObservation copies the array's
        // contents into its own internal buffer synchronously before returning, so the
        // caller's array can be safely overwritten on the next iteration. Allocating here
        // instead would cost roughly 9k Gen0 allocations/sec scene-wide (8 agents x up to
        // 7 others x 16 arenas x 10 decisions/sec) landing straight inside env_step, which
        // Theory §5 measures as 54.5% of wall-clock — exactly the budget Phase C can't spend.
        if (entitySensor != null)
        {
            foreach (TagAgent other in arena.AllActiveAgents())
            {
                if (other == this) continue;
                Vector3 rel = other.transform.localPosition - transform.localPosition;
                Vector3 vel = other.Body.linearVelocity;
                Vector3 fwd = other.transform.forward;
                entityScratch[0] = rel.x;
                entityScratch[1] = rel.y;
                entityScratch[2] = rel.z;
                entityScratch[3] = vel.x;
                entityScratch[4] = vel.y;
                entityScratch[5] = vel.z;
                entityScratch[6] = fwd.x;
                entityScratch[7] = fwd.y;
                entityScratch[8] = fwd.z;
                entityScratch[9] = other.teamId == teamId ? 1f : 0f;   // teammate flag
                entitySensor.AppendObservation(entityScratch);
            }
        }
    }

    // Current potential Φ(s) for the chaser, measured against the NEAREST active runner.
    // Sparse arm (coef 0) makes this a no-op, which is every Phase C run.
    private float CurrentPotential()
    {
        TagAgent opponent = arena.GetNearestOpponent(this);
        if (opponent == null) return 0f;
        return TagReward.Potential(transform.localPosition,
                                   opponent.transform.localPosition,
                                   distanceShapingCoef, arenaDiagonal);
    }

    /// <summary>
    /// Called by TagArenaManager after every reset. Reads the shaping env-params and
    /// seeds Φ from the fresh spawn so the first PBS delta is well-defined.
    /// </summary>
    public void OnArenaReset()
    {
        distanceShapingCoef = Academy.Instance.EnvironmentParameters
            .GetWithDefault("distance_shaping_coef", 0f);
        shapingGamma = Academy.Instance.EnvironmentParameters
            .GetWithDefault("shaping_gamma", shapingGamma);

        if (!shapingParamsLogged)
        {
            Debug.Log($"[TagAgent] distance_shaping_coef={distanceShapingCoef:F2}, " +
                      $"shaping_gamma={shapingGamma:F3}");
            shapingParamsLogged = true;
        }

        prevPotential = CurrentPotential();
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
            AddReward(-0.001f);

            float curPotential = CurrentPotential();
            AddReward(TagReward.ShapingDelta(prevPotential, curPotential, shapingGamma));
            prevPotential = curPotential;
        }
        else // RUNNER
        {
            AddReward(+0.001f);
        }
        // NOTE: arena.Step() is NOT called here any more — TagArenaManager.FixedUpdate
        // owns the clock, so it ticks exactly once per physics step regardless of how
        // many chasers exist.
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