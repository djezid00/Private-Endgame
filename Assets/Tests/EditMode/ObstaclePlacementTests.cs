using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ObstaclePlacementTests
{
    const float HalfSize  = 10f;  // 20x20 floor
    const float WallClear = 1.5f;
    const float MinSep    = 4f;

    [Test]
    public void IsInsideArena_Centre_IsInside()
    {
        Assert.IsTrue(ObstaclePlacement.IsInsideArena(Vector2.zero, HalfSize, WallClear));
    }

    [Test]
    public void IsInsideArena_InsideWallClearance_IsOutside()
    {
        // limit = 10 - 1.5 = 8.5; x = 9 violates the wall clearance band
        Assert.IsFalse(ObstaclePlacement.IsInsideArena(new Vector2(9f, 0f), HalfSize, WallClear));
    }

    [Test]
    public void RespectsSeparation_TooClose_IsFalse()
    {
        var placed = new List<Vector2> { new Vector2(0f, 0f) };
        Assert.IsFalse(ObstaclePlacement.RespectsSeparation(new Vector2(1f, 0f), placed, MinSep));
        Assert.IsTrue (ObstaclePlacement.RespectsSeparation(new Vector2(5f, 0f), placed, MinSep));
    }

    [Test]
    public void TryPlaceObstacles_PlacesCount_AllInvariantsHold()
    {
        var rng = new System.Random(12345);
        var result = new List<Vector2>();
        bool ok = ObstaclePlacement.TryPlaceObstacles(4, HalfSize, WallClear, MinSep, rng, result);

        Assert.IsTrue(ok);
        Assert.AreEqual(4, result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            Assert.IsTrue(ObstaclePlacement.IsInsideArena(result[i], HalfSize, WallClear),
                          $"obstacle {i} at {result[i]} violates arena bounds");
            for (int j = i + 1; j < result.Count; j++)
                Assert.GreaterOrEqual(Vector2.Distance(result[i], result[j]), MinSep,
                                      $"obstacles {i},{j} closer than minSeparation");
        }
    }

    [Test]
    public void TryPlaceObstacles_SameSeed_IsDeterministic()
    {
        var a = new List<Vector2>(); var b = new List<Vector2>();
        ObstaclePlacement.TryPlaceObstacles(4, HalfSize, WallClear, MinSep, new System.Random(7), a);
        ObstaclePlacement.TryPlaceObstacles(4, HalfSize, WallClear, MinSep, new System.Random(7), b);
        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void TryPlaceObstacles_ImpossibleFit_ReturnsFalse()
    {
        var result = new List<Vector2>();
        // minSeparation 100 cannot fit 4 obstacles in a 20x20 arena
        bool ok = ObstaclePlacement.TryPlaceObstacles(4, HalfSize, WallClear, 100f,
                                                      new System.Random(1), result);
        Assert.IsFalse(ok);
    }

    [Test]
    public void IsClearOfObstacles_RespectsActiveCount()
    {
        var obstacles = new List<Vector2> { new Vector2(5f, 5f), new Vector2(-5f, -5f), new Vector2(0f, 0f) };
        var probe = new Vector2(0.5f, 0f); // within 1.5 of obstacle index 2
        Assert.IsFalse(ObstaclePlacement.IsClearOfObstacles(probe, obstacles, 3, 1.5f));
        Assert.IsTrue (ObstaclePlacement.IsClearOfObstacles(probe, obstacles, 2, 1.5f)); // index 2 inactive
    }
}
