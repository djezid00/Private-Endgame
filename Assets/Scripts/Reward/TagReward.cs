using UnityEngine;

/// <summary>
/// Pure, side-effect-free reward math for the Tag game. Kept in its own assembly
/// (TagGame.Reward) so it can be unit-tested in isolation.
/// </summary>
public static class TagReward
{
    /// <summary>Distance in the XZ (floor) plane; ignores Y (agents stay at spawnY).</summary>
    public static float PlanarDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>
    /// Potential Φ(s) = -coef * (dist / maxDist). Closer ⇒ higher (less negative).
    /// coef = 0 ⇒ Φ ≡ 0 (sparse arm, shaping is a no-op).
    /// </summary>
    public static float Potential(Vector3 chaserPos, Vector3 runnerPos, float coef, float maxDist)
    {
        if (coef == 0f) return 0f;
        float dist = PlanarDistance(chaserPos, runnerPos);
        return -coef * (dist / maxDist);
    }

    /// <summary>
    /// Potential-based shaping reward F = γ·Φ(s') - Φ(s) (Ng, Harada & Russell, 1999).
    /// Policy-invariant: changes learning speed, not the optimal policy.
    /// </summary>
    public static float ShapingDelta(float phiPrev, float phiNext, float gamma)
    {
        return gamma * phiNext - phiPrev;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEAM-NORMALIZED TERMINAL SHARES (Phase C)
    //
    // Each runner carries 1/numRunners of the team outcome, so:
    //   all runners caught  => chaser total in [+1, +1.5]
    //   no runners caught   => chaser total exactly -1
    // At numRunners == 1 these reduce ALGEBRAICALLY to the legacy 1v1 formulas,
    // which is what makes the 1v1 regression run a test of the arena-loop
    // refactor rather than of new reward math.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Faster catches pay more: (1 - clamp01(t/max)) * 0.5, range [0, 0.5].</summary>
    public static float TimeBonus(int stepCount, int maxSteps)
    {
        if (maxSteps <= 0) return 0f;
        return (1f - Mathf.Clamp01((float)stepCount / maxSteps)) * 0.5f;
    }

    /// <summary>Surviving longer softens the loss: clamp01(t/max) * 0.5, range [0, 0.5].</summary>
    public static float SurvivalBonus(int stepCount, int maxSteps)
    {
        if (maxSteps <= 0) return 0f;
        return Mathf.Clamp01((float)stepCount / maxSteps) * 0.5f;
    }

    /// <summary>Chaser group's share for catching ONE runner at stepCount. Range [1/numRunners, 1.5/numRunners].</summary>
    public static float CatchShareChaser(int stepCount, int maxSteps, int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return (1f + TimeBonus(stepCount, maxSteps)) / numRunners;
    }

    /// <summary>Runner group's share when ONE of its members is caught at stepCount. Range [-1/numRunners, -0.5/numRunners].</summary>
    public static float CatchShareRunner(int stepCount, int maxSteps, int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return (-1f + SurvivalBonus(stepCount, maxSteps)) / numRunners;
    }

    /// <summary>Chaser group's share for ONE runner that survived to the timeout. Equals -1/numRunners.</summary>
    public static float SurvivalShareChaser(int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return -1f / numRunners;
    }

    /// <summary>Runner group's share for ONE of its members surviving to the timeout. Equals 1/numRunners.</summary>
    public static float SurvivalShareRunner(int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return 1f / numRunners;
    }
}
