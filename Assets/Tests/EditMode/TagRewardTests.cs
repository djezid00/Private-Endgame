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

    // ─── Team-normalized terminal shares (Phase C) ────────────────────────────

    [Test]
    public void TimeBonus_MatchesLegacyScaledFormula()
    {
        // legacy: (1 - clamp01(t/max)) * 0.5
        Assert.AreEqual(0.5f,  TagReward.TimeBonus(0,    2000), 1e-6f);
        Assert.AreEqual(0.25f, TagReward.TimeBonus(1000, 2000), 1e-6f);
        Assert.AreEqual(0f,    TagReward.TimeBonus(2000, 2000), 1e-6f);
        Assert.AreEqual(0f,    TagReward.TimeBonus(9999, 2000), 1e-6f); // clamped
    }

    [Test]
    public void SurvivalBonus_MatchesLegacyScaledFormula()
    {
        Assert.AreEqual(0f,    TagReward.SurvivalBonus(0,    2000), 1e-6f);
        Assert.AreEqual(0.25f, TagReward.SurvivalBonus(1000, 2000), 1e-6f);
        Assert.AreEqual(0.5f,  TagReward.SurvivalBonus(2000, 2000), 1e-6f);
        Assert.AreEqual(0.5f,  TagReward.SurvivalBonus(9999, 2000), 1e-6f); // clamped
    }

    [Test]
    public void CatchShares_AtOneRunner_ReduceToLegacy1v1Values()
    {
        // The exact-reduction property: at N_r = 1 the team formula must equal
        // the legacy expression (1 + timeBonus) / (-1 + survivalBonus).
        for (int t = 0; t <= 2000; t += 137)
        {
            float tb = (1f - Mathf.Clamp01((float)t / 2000f)) * 0.5f;
            float sb = Mathf.Clamp01((float)t / 2000f) * 0.5f;
            Assert.AreEqual( 1f + tb, TagReward.CatchShareChaser(t, 2000, 1), 1e-6f);
            Assert.AreEqual(-1f + sb, TagReward.CatchShareRunner(t, 2000, 1), 1e-6f);
        }
    }

    [Test]
    public void SurvivalShares_AtOneRunner_ReduceToLegacy1v1Values()
    {
        Assert.AreEqual(-1f, TagReward.SurvivalShareChaser(1), 1e-6f);
        Assert.AreEqual( 1f, TagReward.SurvivalShareRunner(1), 1e-6f);
    }

    [Test]
    public void AllRunnersCaught_ChaserTotal_StaysOnUnitScale()
    {
        // 3 runners all caught => chaser total must land in [+1, +1.5]
        foreach (int n in new[] { 1, 2, 3, 4 })
        {
            float total = 0f;
            for (int i = 0; i < n; i++) total += TagReward.CatchShareChaser(500, 2000, n);
            Assert.GreaterOrEqual(total, 1f - 1e-5f);
            Assert.LessOrEqual(total, 1.5f + 1e-5f);
        }
    }

    [Test]
    public void AllRunnersCaught_RunnerTotal_StaysOnUnitScale()
    {
        // Mirror of the chaser check: runner-side total for n catches must land in [-1, -0.5].
        // (NOT the [-1, -1.5] a reader might assume by symmetry — see spec §4 note on the
        // net +0.5/n asymmetry of the catch-bonus mechanics.)
        foreach (int n in new[] { 1, 2, 3, 4 })
        {
            float total = 0f;
            for (int i = 0; i < n; i++) total += TagReward.CatchShareRunner(500, 2000, n);
            Assert.GreaterOrEqual(total, -1f - 1e-5f);
            Assert.LessOrEqual(total, -0.5f + 1e-5f);
        }
    }

    [Test]
    public void NoRunnersCaught_ChaserTotal_IsExactlyMinusOne()
    {
        foreach (int n in new[] { 1, 2, 3, 4 })
        {
            float total = 0f;
            for (int i = 0; i < n; i++) total += TagReward.SurvivalShareChaser(n);
            Assert.AreEqual(-1f, total, 1e-5f);
        }
    }

    [Test]
    public void NoRunnersCaught_RunnerTotal_IsExactlyPlusOne()
    {
        foreach (int n in new[] { 1, 2, 3, 4 })
        {
            float total = 0f;
            for (int i = 0; i < n; i++) total += TagReward.SurvivalShareRunner(n);
            Assert.AreEqual(1f, total, 1e-5f);
        }
    }

    [Test]
    public void PartialOutcomes_AreMonotoneInNumberCaught()
    {
        const int n = 4;
        float prev = float.NegativeInfinity;
        for (int caught = 0; caught <= n; caught++)
        {
            float total = caught * TagReward.CatchShareChaser(500, 2000, n)
                        + (n - caught) * TagReward.SurvivalShareChaser(n);
            Assert.Greater(total, prev);
            prev = total;
        }
    }

    [Test]
    public void Shares_GuardAgainstZeroRunners()
    {
        Assert.AreEqual(0f, TagReward.CatchShareChaser(0, 2000, 0), 1e-6f);
        Assert.AreEqual(0f, TagReward.SurvivalShareChaser(0), 1e-6f);
        Assert.AreEqual(0f, TagReward.CatchShareRunner(0, 2000, 0), 1e-6f);
        Assert.AreEqual(0f, TagReward.SurvivalShareRunner(0), 1e-6f);
    }

    [Test]
    public void Bonuses_GuardAgainstZeroMaxSteps()
    {
        Assert.AreEqual(0f, TagReward.TimeBonus(100, 0), 1e-6f);
        Assert.AreEqual(0f, TagReward.SurvivalBonus(100, 0), 1e-6f);
    }
}
