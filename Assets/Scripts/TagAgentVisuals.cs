using UnityEngine;

// Attach this component to each agent GameObject alongside TagAgent.
// It handles two visual debug features:
//   1. Agent colour coding (red = chaser, blue = runner)
//   2. A direction arrow showing where the agent is currently facing
//
// WHY VISUALISE FACING DIRECTION:
// transform.forward is now one of the 18 observation inputs. During
// debugging it is essential to verify visually that the arrow matches
// the direction the agent is actually moving/turning. If the arrow
// and movement disagree, there is likely a coordinate system mismatch
// in CollectObservations (e.g. localPosition vs worldPosition).
//
// WHY COLOUR CODE:
// With two identical capsule meshes it is impossible to track which
// agent is which during fast training. Distinct colours let you spot
// at a glance whether the chaser is closing in or the runner is
// escaping, and they make screen-recorded thesis footage much clearer.

[RequireComponent(typeof(TagAgent))]
public class TagAgentVisuals : MonoBehaviour
{
    [Header("Colors")]
    public Color chaserColor = new Color(0.9f, 0.2f, 0.2f); // red
    public Color runnerColor  = new Color(0.2f, 0.4f, 0.9f); // blue

    [Header("Direction Arrow")]
    public float arrowLength   = 1.5f;
    public float arrowHeadSize = 0.3f;

    private TagAgent agent;
    private Renderer agentRenderer;

    void Start()
    {
        agent = GetComponent<TagAgent>();
        agentRenderer = GetComponentInChildren<Renderer>();

        ApplyColor();
    }

    void ApplyColor()
    {
        if (agentRenderer == null) return;

        // Create a unique material instance so changing one agent's colour
        // does not affect the other (Unity shares materials by default).
        Material mat = new Material(agentRenderer.sharedMaterial);
        mat.color = (agent.teamId == 0) ? chaserColor : runnerColor;
        agentRenderer.material = mat;
    }

    // OnDrawGizmos runs in the Unity Editor Scene view even during Play mode.
    // It draws the direction arrow without needing any extra GameObjects.
    void OnDrawGizmos()
    {
        if (agent == null) agent = GetComponent<TagAgent>();

        Color arrowColor = (agent != null && agent.teamId == 0)
            ? new Color(1f, 0.3f, 0.3f)   // red arrow for chaser
            : new Color(0.3f, 0.5f, 1f);  // blue arrow for runner

        Gizmos.color = arrowColor;

        Vector3 start = transform.position + Vector3.up * 0.5f; // slightly above agent
        Vector3 end   = start + transform.forward * arrowLength;

        // Arrow shaft
        Gizmos.DrawLine(start, end);

        // Arrow head (two short lines forming a V)
        Vector3 right = Quaternion.Euler(0, 30, 0)  * (-transform.forward) * arrowHeadSize;
        Vector3 left  = Quaternion.Euler(0, -30, 0) * (-transform.forward) * arrowHeadSize;
        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
}