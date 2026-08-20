using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure, side-effect-free spawn placement for N chasers + M runners in the Tag arena.
/// Lives in the TagGame.Reward assembly so it can be unit-tested without scene objects.
/// Positions are arena-local XZ (Vector2.y = Z); the caller supplies spawnY.
/// Takes System.Random (not UnityEngine.Random) so tests can seed it deterministically.
///
/// Chasers occupy the LEFT half, runners the RIGHT half — separated sides prevent
/// instant-collision on spawn, exactly as the legacy 1v1 code did.
/// </summary>
public static class SpawnPlacement
{
    /// <summary>Rejection-sampling attempts per agent before the whole sample fails.</summary>
    public const int MaxAttemptsPerAgent = 60;

    /// <summary>
    /// Samples chaserCount + runnerCount positions. Result order is chasers first, then
    /// runners. Returns false with `result` left EMPTY if any agent exceeds its attempt
    /// budget, so callers can react rather than silently spawning agents inside each other.
    /// </summary>
    public static bool TrySampleSpawns(int chaserCount, int runnerCount,
                                       float arenaRadius, float minSpawnDistance,
                                       IReadOnlyList<Vector2> obstacles, int obstacleCount,
                                       float obstacleClearance,
                                       System.Random rng, List<Vector2> result)
    {
        result.Clear();
        float edge = arenaRadius - 1f;

        for (int i = 0; i < chaserCount + runnerCount; i++)
        {
            bool isChaser = i < chaserCount;
            float xMin = isChaser ? -edge : 1f;
            float xMax = isChaser ? -1f   : edge;

            bool placed = false;
            for (int attempt = 0; attempt < MaxAttemptsPerAgent && !placed; attempt++)
            {
                var candidate = new Vector2(
                    xMin + (float)rng.NextDouble() * (xMax - xMin),
                    -edge + (float)rng.NextDouble() * (2f * edge));

                if (!ObstaclePlacement.RespectsSeparation(candidate, result, minSpawnDistance))
                    continue;
                if (!ObstaclePlacement.IsClearOfObstacles(candidate, obstacles, obstacleCount,
                                                          obstacleClearance))
                    continue;

                result.Add(candidate);
                placed = true;
            }
            if (!placed) { result.Clear(); return false; }
        }
        return true;
    }
}
