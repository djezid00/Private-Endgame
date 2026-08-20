using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SpawnPlacementTests
{
    const float Radius    = 8f;
    const float MinSep    = 3f;
    const float Clearance = 1.5f;

    static List<Vector2> NoObstacles() => new List<Vector2>();

    [Test]
    public void TrySample_1v1_Succeeds_AndSeparatesSides()
    {
        var rng = new System.Random(1);
        var result = new List<Vector2>();
        bool ok = SpawnPlacement.TrySampleSpawns(1, 1, Radius, MinSep,
                                                 NoObstacles(), 0, Clearance, rng, result);
        Assert.IsTrue(ok);
        Assert.AreEqual(2, result.Count);
        Assert.Less(result[0].x, 0f);      // chaser on the left
        Assert.Greater(result[1].x, 0f);   // runner on the right
    }

    [Test]
    public void TrySample_RespectsPairwiseSeparation()
    {
        var rng = new System.Random(7);
        var result = new List<Vector2>();
        Assert.IsTrue(SpawnPlacement.TrySampleSpawns(3, 3, Radius, MinSep,
                                                     NoObstacles(), 0, Clearance, rng, result));
        for (int i = 0; i < result.Count; i++)
            for (int j = i + 1; j < result.Count; j++)
                Assert.GreaterOrEqual(Vector2.Distance(result[i], result[j]), MinSep - 1e-4f);
    }

    [Test]
    public void TrySample_StaysInsideArena()
    {
        var rng = new System.Random(11);
        var result = new List<Vector2>();
        Assert.IsTrue(SpawnPlacement.TrySampleSpawns(2, 2, Radius, MinSep,
                                                     NoObstacles(), 0, Clearance, rng, result));
        foreach (var p in result)
        {
            Assert.LessOrEqual(Mathf.Abs(p.x), Radius - 1f + 1e-4f);
            Assert.LessOrEqual(Mathf.Abs(p.y), Radius - 1f + 1e-4f);
        }
    }

    [Test]
    public void TrySample_AvoidsObstacles()
    {
        var obstacles = new List<Vector2> {
            new Vector2(-4f, 0f), new Vector2(4f, 0f),
            new Vector2(0f, -4f), new Vector2(0f, 4f)
        };
        var rng = new System.Random(3);
        var result = new List<Vector2>();
        Assert.IsTrue(SpawnPlacement.TrySampleSpawns(2, 2, Radius, MinSep,
                                                     obstacles, obstacles.Count, Clearance, rng, result));
        foreach (var p in result)
            foreach (var o in obstacles)
                Assert.GreaterOrEqual(Vector2.Distance(p, o), Clearance - 1e-4f);
    }

    [Test]
    public void TrySample_ClearsResultOnFailure()
    {
        // 20 agents per side at minSep 3 cannot fit — must fail cleanly with an empty list.
        var rng = new System.Random(5);
        var result = new List<Vector2>();
        bool ok = SpawnPlacement.TrySampleSpawns(20, 20, Radius, MinSep,
                                                 NoObstacles(), 0, Clearance, rng, result);
        Assert.IsFalse(ok);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void TrySample_MaxComposition_4v4_WithFourPillars_IsFeasible()
    {
        // HARD CONSTRAINT on the run matrix: if this fails, 4v4 is not runnable and the
        // maximum composition must be lowered to whatever does pass.
        var obstacles = new List<Vector2> {
            new Vector2(-4f, -4f), new Vector2(4f, -4f),
            new Vector2(-4f, 4f),  new Vector2(4f, 4f)
        };
        int successes = 0;
        for (int seed = 0; seed < 50; seed++)
        {
            var rng = new System.Random(seed);
            var result = new List<Vector2>();
            if (SpawnPlacement.TrySampleSpawns(4, 4, Radius, MinSep,
                                               obstacles, obstacles.Count, Clearance, rng, result))
                successes++;
        }
        // System.Random(seed) is fully deterministic per seed, so there is no run-to-run
        // noise across seeds 0..49 for slack to absorb — require all 50, not a fuzzy majority.
        Assert.AreEqual(50, successes,
            $"4v4 spawn feasible in only {successes}/50 seeds (expected all 50) — lower the max composition.");
    }

    [Test]
    public void TrySample_ZeroAgents_SucceedsEmpty()
    {
        var rng = new System.Random(1);
        var result = new List<Vector2>();
        bool ok = SpawnPlacement.TrySampleSpawns(0, 0, Radius, MinSep,
                                                 NoObstacles(), 0, Clearance, rng, result);
        Assert.IsTrue(ok);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void TrySample_ClearsStaleResultBeforeSuccess()
    {
        var rng = new System.Random(1); // same seed as TrySample_1v1_Succeeds_AndSeparatesSides
        var result = new List<Vector2> { new Vector2(99f, 99f) }; // stale content must be cleared
        bool ok = SpawnPlacement.TrySampleSpawns(1, 1, Radius, MinSep,
                                                 NoObstacles(), 0, Clearance, rng, result);
        Assert.IsTrue(ok);
        Assert.AreEqual(2, result.Count);
        CollectionAssert.DoesNotContain(result, new Vector2(99f, 99f));
    }

    [Test]
    public void TrySample_IsDeterministicForSameSeed()
    {
        var a = new List<Vector2>();
        var b = new List<Vector2>();
        SpawnPlacement.TrySampleSpawns(2, 2, Radius, MinSep, NoObstacles(), 0, Clearance,
                                       new System.Random(42), a);
        SpawnPlacement.TrySampleSpawns(2, 2, Radius, MinSep, NoObstacles(), 0, Clearance,
                                       new System.Random(42), b);
        CollectionAssert.AreEqual(a, b);
    }
}
