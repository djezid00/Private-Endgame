using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure, side-effect-free obstacle placement math for the Tag arena. Lives in the
/// TagGame.Reward assembly (the project's unit-tested pure-logic assembly) so it can
/// be tested without scene objects. Positions are arena-local XZ (Vector2.y = Z).
/// Takes System.Random (not UnityEngine.Random) so tests can seed it deterministically.
/// </summary>
public static class ObstaclePlacement
{
    /// <summary>Rejection-sampling attempts per obstacle before giving up.</summary>
    public const int MaxAttemptsPerObstacle = 50;

    /// <summary>Inside the square arena, keeping wallClearance away from every wall.</summary>
    public static bool IsInsideArena(Vector2 posXZ, float arenaHalfSize, float wallClearance)
    {
        float limit = arenaHalfSize - wallClearance;
        return Mathf.Abs(posXZ.x) <= limit && Mathf.Abs(posXZ.y) <= limit;
    }

    /// <summary>At least minSeparation from every already-placed obstacle.</summary>
    public static bool RespectsSeparation(Vector2 candidate, IReadOnlyList<Vector2> placed,
                                          float minSeparation)
    {
        for (int i = 0; i < placed.Count; i++)
            if (Vector2.Distance(candidate, placed[i]) < minSeparation) return false;
        return true;
    }

    /// <summary>
    /// Rejection-samples `count` positions satisfying bounds + separation. Returns false
    /// (partial result cleared by caller policy) if any obstacle exceeds its attempt budget —
    /// the caller falls back to the authored fixed layout rather than break training.
    /// </summary>
    public static bool TryPlaceObstacles(int count, float arenaHalfSize, float wallClearance,
                                         float minSeparation, System.Random rng,
                                         List<Vector2> result)
    {
        result.Clear();
        float limit = arenaHalfSize - wallClearance;
        for (int i = 0; i < count; i++)
        {
            bool placed = false;
            for (int attempt = 0; attempt < MaxAttemptsPerObstacle && !placed; attempt++)
            {
                var candidate = new Vector2(
                    (float)(rng.NextDouble() * 2.0 - 1.0) * limit,
                    (float)(rng.NextDouble() * 2.0 - 1.0) * limit);
                if (RespectsSeparation(candidate, result, minSeparation))
                {
                    result.Add(candidate);
                    placed = true;
                }
            }
            if (!placed) return false;
        }
        return true;
    }

    /// <summary>Is posXZ at least `clearance` from the first `activeCount` obstacles?</summary>
    public static bool IsClearOfObstacles(Vector2 posXZ, IReadOnlyList<Vector2> obstacles,
                                          int activeCount, float clearance)
    {
        int n = Mathf.Min(activeCount, obstacles.Count);
        for (int i = 0; i < n; i++)
            if (Vector2.Distance(posXZ, obstacles[i]) < clearance) return false;
        return true;
    }
}
