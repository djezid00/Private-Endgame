using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TagAgent : Agent
{
    public TagArenaManager arena;
    public int teamId;             // 0 = chaser, 1 = runner
    public float moveSpeed = 5f;
    public float turnSpeed = 180f;

    Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        arena.ResetArena();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // --- SELF ---
        // 1) Own position (3 floats)
        //    WHY: The agent must know where it is in the arena to plan movement.
        sensor.AddObservation(transform.localPosition);

        // 2) Own velocity (3 floats)
        //    WHY: Velocity tells the agent how fast and in what direction it is
        //    currently moving, which is critical for momentum-based decisions
        //    like braking before walls or accelerating toward the opponent.
        sensor.AddObservation(rb.linearVelocity);

        // 3) Own facing direction (3 floats)
        //    WHY (NEW): Without knowing which way it is facing, the agent cannot
        //    distinguish "move forward" from "move backward". transform.forward
        //    encodes the agent's heading as a normalised 3D vector. This is the
        //    most common missing observation in beginner ML-Agents setups.
        sensor.AddObservation(transform.forward);

        // --- OPPONENT ---
        var opponent = arena.GetOpponent(this);

        // 4) Relative opponent position (3 floats)
        //    WHY: Using the difference vector instead of the raw world position
        //    makes the observation translation-invariant — it tells the agent
        //    "the opponent is X units ahead and Y units to my left" rather than
        //    abstract world coordinates, which speeds up learning.
        Vector3 relativePos = opponent.transform.localPosition - transform.localPosition;
        sensor.AddObservation(relativePos);

        // 5) Opponent velocity (3 floats)
        //    WHY (NEW): Knowing where the opponent IS right now is not enough;
        //    the agent also needs to know where the opponent WILL BE in the next
        //    few frames. Opponent velocity enables predictive interception
        //    (chaser) and evasive manoeuvring (runner) — two core competitive
        //    behaviours described in your thesis topic.
        Rigidbody opponentRb = opponent.GetComponent<Rigidbody>();
        sensor.AddObservation(opponentRb.linearVelocity);

        // 6) Opponent facing direction (3 floats)
        //    WHY (NEW): The direction the opponent is looking reveals their
        //    likely next move. A runner facing away is about to flee; a chaser
        //    facing toward you is about to charge. This is especially useful
        //    for the runner learning to anticipate the chaser's trajectory.
        sensor.AddObservation(opponent.transform.forward);

        // TOTAL: 3+3+3 (self) + 3+3+3 (opponent) = 18 floats per step
        // Previous total was 9 floats — doubled the information density.
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float turn = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        rb.MovePosition(transform.position + transform.forward * move * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.fixedDeltaTime);

        // Small per-step penalty to encourage efficiency (unchanged)
        AddReward(-0.001f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Agent"))
        {
            arena.OnAgentTagged(this, collision.collider.GetComponent<TagAgent>());
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
       // OLD (while training):
    // c[0] = Input.GetAxis("Vertical");
    // c[1] = Input.GetAxis("Horizontal");

    // NEW — after it has been trained:
    var c = actionsOut.ContinuousActions;
    c[0] = UnityEngine.InputSystem.Keyboard.current.wKey.isPressed ? 1f :
           UnityEngine.InputSystem.Keyboard.current.sKey.isPressed ? -1f : 0f;
    c[1] = UnityEngine.InputSystem.Keyboard.current.dKey.isPressed ? 1f :
           UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ? -1f : 0f;
    }
}