using NUnit.Framework;
using UnityEngine;

public class TagRewardTests
{
    const float MaxDist = 28.28f;
    const float Coef    = 0.5f;
    const float Gamma   = 0.99f;

    [Test]
    public void PlanarDistance_IgnoresY()
    {
        float d = TagReward.PlanarDistance(new Vector3(0f, 0f, 0f), new Vector3(0f, 5f, 0f));
        Assert.AreEqual(0f, d, 1e-5f);
    }

    [Test]
    public void Potential_Closer_IsGreaterThan_Farther()
    {
        float near = TagReward.Potential(Vector3.zero, new Vector3(1f, 0.5f, 0f), Coef, MaxDist);
        float far  = TagReward.Potential(Vector3.zero, new Vector3(10f, 0.5f, 0f), Coef, MaxDist);
        Assert.Greater(near, far);
    }

    [Test]
    public void Potential_ZeroCoef_IsAlwaysZero()
    {
        float p = TagReward.Potential(Vector3.zero, new Vector3(7f, 0f, 3f), 0f, MaxDist);
        Assert.AreEqual(0f, p, 1e-6f);
    }

    [Test]
    public void ShapingDelta_Closing_IsPositive()
    {
        float phiPrev = TagReward.Potential(Vector3.zero, new Vector3(10f, 0f, 0f), Coef, MaxDist);
        float phiNext = TagReward.Potential(Vector3.zero, new Vector3(5f, 0f, 0f), Coef, MaxDist);
        Assert.Greater(TagReward.ShapingDelta(phiPrev, phiNext, Gamma), 0f);
    }

    [Test]
    public void ShapingDelta_Receding_IsNegative()
    {
        float phiPrev = TagReward.Potential(Vector3.zero, new Vector3(5f, 0f, 0f), Coef, MaxDist);
        float phiNext = TagReward.Potential(Vector3.zero, new Vector3(10f, 0f, 0f), Coef, MaxDist);
        Assert.Less(TagReward.ShapingDelta(phiPrev, phiNext, Gamma), 0f);
    }
}
