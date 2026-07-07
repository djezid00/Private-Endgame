using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Drives the user-authored obstacle pillars per episode from two trainer env-params:
///   num_obstacles   (default 0)  — how many pillars are active; 0 = legacy open arena.
///   obstacle_layout (default 0)  — 0 = user's authored (fixed) layout, 1 = randomize
///                                  position + Y-rotation each episode.
/// Attach to the "Obstacles" parent inside the TagArena prefab; pillars are its children
/// (tag "Wall", static BoxColliders, ~2u tall). Placement math is ObstaclePlacement
/// (unit-tested); on a random-placement failure the authored layout is used instead —
/// never break training. TryPlaceObstacles leaves `positions` empty on failure, so the
/// fixed-layout fallback below always starts from a clean list.
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("Authored pillars (auto-collected from children if left empty)")]
    public Transform[] pillars;

    [Header("Placement rules (random mode)")]
    public float arenaHalfSize  = 10f;  // 20x20 floor
    public float wallClearance  = 1.5f;
    public float minSeparation  = 4f;
    public float agentClearance = 1.5f; // used by TagArenaManager spawn rejection

    private System.Random rng;  // seeded per-instance in Awake — 16 arenas must not share a layout stream
    private readonly List<Vector2> positions = new List<Vector2>(); // active obstacle XZ
    private Vector3[]    authoredLocalPos;
    private Quaternion[] authoredLocalRot;

    private static bool paramsLogged = false;
    private static int  instanceCounter = 0; // per-instance RNG stream id (see Awake)

    private void Awake()
    {
        // Environment.TickCount has ~15ms resolution: all 16 arena managers construct in the
        // same scene-load tick, so a default seed would give every arena the SAME layout
        // sequence forever. Mix in a unique per-instance counter for per-arena streams.
        // (A counter instead of GetInstanceID(): that API is deprecated in Unity 6, CS0618.)
        rng = new System.Random(unchecked(System.Environment.TickCount * 397 ^ instanceCounter++));

        if (pillars == null || pillars.Length == 0)
        {
            pillars = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                pillars[i] = transform.GetChild(i);
        }
        authoredLocalPos = new Vector3[pillars.Length];
        authoredLocalRot = new Quaternion[pillars.Length];
        for (int i = 0; i < pillars.Length; i++)
        {
            authoredLocalPos[i] = pillars[i].localPosition;
            authoredLocalRot[i] = pillars[i].localRotation;
        }
    }

    /// <summary>Called by TagArenaManager.ResetArena() BEFORE agents spawn.</summary>
    public void ResetObstacles()
    {
        int active = Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                         .GetWithDefault("num_obstacles", 0f)), 0, pillars.Length);
        bool random = Academy.Instance.EnvironmentParameters
                         .GetWithDefault("obstacle_layout", 0f) > 0.5f;

        if (!paramsLogged)
        {
            Debug.Log($"[ObstacleManager] num_obstacles={active}, layout={(random ? "random" : "fixed")}");
            paramsLogged = true;
        }

        for (int i = 0; i < pillars.Length; i++)
            pillars[i].gameObject.SetActive(i < active);

        positions.Clear();
        if (active == 0) return;

        if (random && ObstaclePlacement.TryPlaceObstacles(active, arenaHalfSize, wallClearance,
                                                          minSeparation, rng, positions))
        {
            for (int i = 0; i < active; i++)
            {
                pillars[i].localPosition = new Vector3(positions[i].x,
                                                       authoredLocalPos[i].y,
                                                       positions[i].y);
                pillars[i].localRotation = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);
            }
        }
        else
        {
            // Fixed mode — or random-placement failure fallback: the authored layout.
            // (positions is empty in both cases: cleared above / cleared by TryPlaceObstacles.)
            for (int i = 0; i < active; i++)
            {
                pillars[i].localPosition = authoredLocalPos[i];
                pillars[i].localRotation = authoredLocalRot[i];
                positions.Add(new Vector2(authoredLocalPos[i].x, authoredLocalPos[i].z));
            }
        }

        // autoSyncTransforms is off in this project: without an explicit sync, the first
        // observation of the new episode would raycast against stale pillar positions.
        // Sits after BOTH branches so the rare random-failure fallback is covered too.
        Physics.SyncTransforms();
    }

    /// <summary>Spawn-safety query for TagArenaManager (arena-local position).</summary>
    public bool IsClearOfActiveObstacles(Vector3 localPos)
    {
        return ObstaclePlacement.IsClearOfObstacles(new Vector2(localPos.x, localPos.z),
                                                    positions, positions.Count, agentClearance);
    }
}
