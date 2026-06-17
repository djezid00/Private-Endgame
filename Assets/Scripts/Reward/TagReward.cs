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
}
