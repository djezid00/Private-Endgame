# Phase C — Multi-Agent Teams Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generalize the 1v1 Tag environment to N-vs-M teams so MA-POCA's centralized counterfactual baseline and posthumous credit assignment finally engage, enabling a MA-POCA-vs-PPO comparison at group size > 1.

**Architecture:** Approach A — generalize `TagArenaManager` and `TagAgent` in place rather than adding parallel classes. Two new pure, unit-tested modules (`SpawnPlacement`, extended `TagReward`) plus one new MonoBehaviour (`TeamManager`) that mirrors the existing `ObstacleManager`. Team sizes come from env-params `num_chasers`/`num_runners`, both defaulting to 1, so every existing config stays byte-identical.

**Tech Stack:** Unity 6000.4.0f1, ML-Agents (local package at `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents`), C#, NUnit EditMode tests, Python 3.12 for analysis (system Python — no conda needed).

**Spec:** `docs/superpowers/specs/2026-08-20-phase-c-multiagent-design.md`
**Branch:** `feat/phase-c-multiagent`

---

## Conventions for the implementing engineer

**Running EditMode tests:** Unity Editor → `Window > General > Test Runner` → `EditMode` tab → `Run All`. There is no CLI test runner configured in this project. Expected: all tests green. The 13 pre-existing tests (8 `ObstaclePlacementTests`, 5 `TagRewardTests`) **must stay green throughout** — they are the regression guard proving 1v1 behaviour is unchanged.

**You cannot run `mlagents-learn` yourself.** conda is not on PATH in a normal shell; it only works in the Anaconda Prompt, which the user drives. Tasks marked **[USER]** are handed to the user with an exact command to paste.

**Commit after every task.** Never `git add -A` — stage explicit paths only (the user keeps untracked drafts in `docs/`).

**Do not add an AI co-author trailer to commit messages.**

---

## File Structure

| File | Responsibility | Status |
|---|---|---|
| `Assets/Scripts/Reward/TagReward.cs` | Pure reward math; gains team-normalized terminal shares | modify |
| `Assets/Scripts/Reward/SpawnPlacement.cs` | Pure N-agent spawn sampling with pairwise + obstacle rejection | **create** |
| `Assets/Scripts/TeamManager.cs` | Activates N chasers + M runners from env-params | **create** |
| `Assets/Scripts/TagAgent.cs` | Observations (VectorSensor + BufferSensor); stops driving clock/reset | modify |
| `Assets/Scripts/TagArenaManager.cs` | Lists per role, own step clock, reset, termination, rewards, stats | modify |
| `Assets/Tests/EditMode/TagRewardTests.cs` | Existing 5 tests + new team-math tests | modify |
| `Assets/Tests/EditMode/SpawnPlacementTests.cs` | Tests for the new spawn module | **create** |
| `Assets/Prefabs/TagArena.prefab` | Carries 4 chasers + 4 runners authored inactive + TeamManager | modify **[USER]** |
| `Assets/Prefabs/{Chaser,Runner}Agent.prefab` | Gain `BufferSensorComponent` | modify **[USER]** |
| `experiments/gen_team_configs.py` | Generates the Phase C YAMLs | **create** |
| `experiments/run_phaseC_smoke.bat` | Smoke gate batch | **create** |

---

## Observation design (read before Task 4)

The vector observation **stays at 18 floats** so that at 1v1 the agent sees byte-identical values to every previous experiment. The buffer is simply empty at 1v1.

**VectorSensor — 18 floats, unchanged semantics:**
```
[0..2]   self localPosition
[3..5]   self linearVelocity
[6..8]   self forward
[9..11]  nearest ACTIVE opponent: relative localPosition (theirs − mine)
[12..14] nearest ACTIVE opponent: linearVelocity
[15..17] nearest ACTIVE opponent: forward
```
If no active opponent remains (all runners caught, chaser still observing before reset), slots 9–17 are zeros.

**BufferSensorComponent — every OTHER active agent, 10 floats each, max 7 entities:**
```
[0..2] relative localPosition (theirs − mine)
[3..5] their linearVelocity
[6..8] their forward
[9]    1.0 if teammate, 0.0 if opponent
```

The nearest opponent appears in **both** the vector obs and the buffer. That redundancy is deliberate: it is what preserves exact 1v1 observation compatibility. `ObservableSize = 10`, `MaxNumObservables = 7` (8 agents max, minus self).

**Caveat to record in the write-up:** at 1v1 the *values* are identical to prior experiments, but the behavior spec now contains an (empty) buffer sensor, so the network has an attention encoder it did not have before. The regression run therefore validates the arena loop, reward math and spawn logic — **not** "same network".

---

## Task 1: Team-normalized reward math

**Files:**
- Modify: `Assets/Scripts/Reward/TagReward.cs`
- Test: `Assets/Tests/EditMode/TagRewardTests.cs`

The existing 1v1 code in `TagArenaManager.OnAgentTagged` computes bonuses as:
```csharp
float taggerProgress = Mathf.Clamp01((float)tagger.StepCount / taggerMax);
float timeBonus      = (1f - taggerProgress) * 0.5f;
float survivalBonus  = taggedProgress * 0.5f;
```
Note this is **scaled by 0.5**, not clamped at 0.5. `CLAUDE.md` and the spec both describe it as `clamp(..., 0, 0.5)`, which is wrong and diverges at intermediate `t`. The functions below reproduce the **code's** behaviour, which is what makes the `N_r = 1` reduction exact.

- [ ] **Step 1: Write the failing tests**

Append to `Assets/Tests/EditMode/TagRewardTests.cs` (inside the existing class, before the closing brace):

```csharp
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
    }
```

- [ ] **Step 2: Run tests to verify they fail**

Unity Editor → `Window > General > Test Runner` → `EditMode` → `Run All`.
Expected: the 8 new tests fail to **compile** ("`TagReward` does not contain a definition for `TimeBonus`"). The Test Runner will show a compile error rather than red tests — that is the expected failure state.

- [ ] **Step 3: Write the implementation**

Append to `Assets/Scripts/Reward/TagReward.cs`, inside the `TagReward` class before the closing brace:

```csharp
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

    /// <summary>Chaser group's share for catching ONE runner at stepCount.</summary>
    public static float CatchShareChaser(int stepCount, int maxSteps, int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return (1f + TimeBonus(stepCount, maxSteps)) / numRunners;
    }

    /// <summary>Runner group's share when ONE of its members is caught at stepCount.</summary>
    public static float CatchShareRunner(int stepCount, int maxSteps, int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return (-1f + SurvivalBonus(stepCount, maxSteps)) / numRunners;
    }

    /// <summary>Chaser group's share for ONE runner that survived to the timeout.</summary>
    public static float SurvivalShareChaser(int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return -1f / numRunners;
    }

    /// <summary>Runner group's share for ONE of its members surviving to the timeout.</summary>
    public static float SurvivalShareRunner(int numRunners)
    {
        if (numRunners <= 0) return 0f;
        return 1f / numRunners;
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Test Runner → `EditMode` → `Run All`.
Expected: **21 tests pass** (13 pre-existing + 8 new). If any of the 13 pre-existing tests fails, stop — the change was not additive.

- [ ] **Step 5: Fix the two stale docs describing the bonus formula**

In `CLAUDE.md`, find the `TagArenaManager.cs — Key Facts` section and replace these two lines:

```
  - `timeBonus = clamp(1 - steps/maxSteps, 0, 0.5)` — reward faster catches
  - `survivalBonus = clamp(steps/maxSteps, 0, 0.5)` — soften penalty for surviving longer
```

with:

```
  - `timeBonus = (1 - clamp01(steps/maxSteps)) * 0.5` — reward faster catches (SCALED by 0.5, not clamped at it)
  - `survivalBonus = clamp01(steps/maxSteps) * 0.5` — soften penalty for surviving longer
```

In `docs/superpowers/specs/2026-08-20-phase-c-multiagent-design.md`, section 4, replace:

```
    timeBonus     = clamp(1 − t/maxEpisodeSteps, 0, 0.5)
    survivalBonus = clamp(t/maxEpisodeSteps, 0, 0.5)
```

with:

```
    timeBonus     = (1 − clamp01(t/maxEpisodeSteps)) * 0.5
    survivalBonus = clamp01(t/maxEpisodeSteps) * 0.5
```

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Reward/TagReward.cs Assets/Tests/EditMode/TagRewardTests.cs CLAUDE.md docs/superpowers/specs/2026-08-20-phase-c-multiagent-design.md
git commit -m "feat: team-normalized terminal reward shares, exact at N_r=1"
```

---

## Task 2: N-agent spawn placement

**Files:**
- Create: `Assets/Scripts/Reward/SpawnPlacement.cs`
- Create: `Assets/Tests/EditMode/SpawnPlacementTests.cs`

Chasers spawn on the left half (x ∈ [−arenaRadius+1, −1]), runners on the right (x ∈ [1, arenaRadius−1]), both with z ∈ [−arenaRadius+1, arenaRadius−1]. Every agent must be ≥ `minSpawnDistance` from every other agent and ≥ `obstacleClearance` from every active obstacle.

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/SpawnPlacementTests.cs`:

```csharp
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
        Assert.GreaterOrEqual(successes, 45,
            $"4v4 spawn feasible in only {successes}/50 seeds — lower the max composition.");
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
```

- [ ] **Step 2: Run tests to verify they fail**

Test Runner → `EditMode` → `Run All`.
Expected: compile error, "The name `SpawnPlacement` does not exist in the current context".

- [ ] **Step 3: Write the implementation**

Create `Assets/Scripts/Reward/SpawnPlacement.cs`:

```csharp
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
```

- [ ] **Step 4: Run tests to verify they pass**

Test Runner → `EditMode` → `Run All`.
Expected: **28 tests pass** (13 pre-existing + 8 from Task 1 + 7 new).

If `TrySample_MaxComposition_4v4_WithFourPillars_IsFeasible` fails, **do not weaken the test.** Record the highest composition that does pass and report it — it becomes a hard cap on the run matrix in Task 13.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Reward/SpawnPlacement.cs Assets/Tests/EditMode/SpawnPlacementTests.cs
git commit -m "feat: pure N-agent spawn placement with pairwise and obstacle rejection"
```

---

## Task 3: TeamManager

**Files:**
- Create: `Assets/Scripts/TeamManager.cs`

Mirrors `ObstacleManager`: reads env-params, activates a prefix of the authored agent list, logs once.

- [ ] **Step 1: Write the implementation**

Create `Assets/Scripts/TeamManager.cs`:

```csharp
using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Activates N chasers + M runners per episode from two trainer env-params:
///   num_chasers (default 1)
///   num_runners (default 1)
/// Both default to 1 so every pre-Phase-C config produces byte-identical behaviour.
///
/// Attach to the TagArena prefab root alongside TagArenaManager. The authored agent
/// arrays hold up to 4 of each, all inactive in the prefab; this component activates
/// a prefix of each array. Mirrors ObstacleManager's design deliberately.
/// </summary>
public class TeamManager : MonoBehaviour
{
    [Header("Authored agents (drag all 4 chasers and all 4 runners)")]
    public TagAgent[] chasers;
    public TagAgent[] runners;

    private static bool paramsLogged = false;

    /// <summary>Chasers active this episode. Valid after ApplyTeamSizes().</summary>
    public int ActiveChasers { get; private set; } = 1;

    /// <summary>Runners active this episode. Valid after ApplyTeamSizes().</summary>
    public int ActiveRunners { get; private set; } = 1;

    /// <summary>
    /// Reads the env-params and activates the corresponding prefix of each array.
    /// Called by TagArenaManager.ResetArena() BEFORE spawn placement.
    /// </summary>
    public void ApplyTeamSizes()
    {
        ActiveChasers = Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                            .GetWithDefault("num_chasers", 1f)), 1, chasers.Length);
        ActiveRunners = Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                            .GetWithDefault("num_runners", 1f)), 1, runners.Length);

        if (!paramsLogged)
        {
            Debug.Log($"[TeamManager] num_chasers={ActiveChasers}, num_runners={ActiveRunners}");
            paramsLogged = true;
        }

        for (int i = 0; i < chasers.Length; i++)
            chasers[i].gameObject.SetActive(i < ActiveChasers);
        for (int i = 0; i < runners.Length; i++)
            runners[i].gameObject.SetActive(i < ActiveRunners);
    }
}
```

- [ ] **Step 2: Verify it compiles**

Return to the Unity Editor and wait for the recompile to finish. Check the Console.
Expected: no compile errors. (`TagAgent` is referenced but unchanged so far.)

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/TeamManager.cs
git commit -m "feat: TeamManager activates N chasers and M runners from env-params"
```

---

## Task 4: TagAgent observations

**Files:**
- Modify: `Assets/Scripts/TagAgent.cs`

- [ ] **Step 1: Replace CollectObservations**

In `Assets/Scripts/TagAgent.cs`, replace the entire `CollectObservations` method with:

```csharp
    public override void CollectObservations(VectorSensor sensor)
    {
        // ── SELF (9 floats) ──────────────────────────────────────────────────
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        sensor.AddObservation(transform.forward);

        // ── NEAREST ACTIVE OPPONENT (9 floats) ───────────────────────────────
        // Kept in the vector observation so that at 1v1 the agent sees byte-identical
        // values to every pre-Phase-C experiment (the buffer below is empty there).
        TagAgent opponent = arena.GetNearestOpponent(this);
        if (opponent != null)
        {
            sensor.AddObservation(opponent.transform.localPosition - transform.localPosition);
            sensor.AddObservation(opponent.GetComponent<Rigidbody>().linearVelocity);
            sensor.AddObservation(opponent.transform.forward);
        }
        else
        {
            // All opponents caught; episode is about to end. Zeros keep the size fixed.
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
        }
        // TOTAL vector: 18 floats — unchanged from 1v1.

        // ── ALL OTHER ACTIVE AGENTS (BufferSensor, 10 floats each) ───────────
        if (entitySensor != null)
        {
            foreach (TagAgent other in arena.AllActiveAgents())
            {
                if (other == this) continue;
                Vector3 rel = other.transform.localPosition - transform.localPosition;
                Vector3 vel = other.GetComponent<Rigidbody>().linearVelocity;
                Vector3 fwd = other.transform.forward;
                entitySensor.AppendObservation(new float[]
                {
                    rel.x, rel.y, rel.z,
                    vel.x, vel.y, vel.z,
                    fwd.x, fwd.y, fwd.z,
                    other.teamId == teamId ? 1f : 0f   // teammate flag
                });
            }
        }
    }
```

- [ ] **Step 2: Cache the buffer sensor and drop the arena-driving code**

In the same file, replace the `Initialize` method with:

```csharp
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        entitySensor = GetComponent<BufferSensorComponent>();
    }
```

Add this field next to `private Rigidbody rb;`:

```csharp
    private BufferSensorComponent entitySensor;
```

Replace the entire `OnEpisodeBegin` method with:

```csharp
    // Arena reset and the step clock are owned by TagArenaManager (Phase C).
    // Agents no longer drive either — with N chasers there is no privileged one,
    // and this also fixes the old ordering bug where the runner was repositioned
    // before its own group episode ended.
    public override void OnEpisodeBegin() { }
```

In `OnActionReceived`, replace the whole role-based reward block (from `if (teamId == 0) // CHASER` through the closing brace of the `else` branch) with:

```csharp
        if (teamId == 0) // CHASER
        {
            AddReward(-0.001f);

            float curPotential = CurrentPotential();
            AddReward(TagReward.ShapingDelta(prevPotential, curPotential, shapingGamma));
            prevPotential = curPotential;
        }
        else // RUNNER
        {
            AddReward(+0.001f);
        }
        // NOTE: arena.Step() is NOT called here any more — TagArenaManager.FixedUpdate
        // owns the clock, so it ticks exactly once per physics step regardless of how
        // many chasers exist.
```

- [ ] **Step 3: Update the shaping helpers for multiple opponents**

Replace `CurrentPotential` with:

```csharp
    // Current potential Φ(s) for the chaser, measured against the NEAREST active runner.
    // Sparse arm (coef 0) makes this a no-op, which is every Phase C run.
    private float CurrentPotential()
    {
        TagAgent opponent = arena.GetNearestOpponent(this);
        if (opponent == null) return 0f;
        return TagReward.Potential(transform.localPosition,
                                   opponent.transform.localPosition,
                                   distanceShapingCoef, arenaDiagonal);
    }
```

Add a public method so the arena can seed shaping state after a reset (previously done inside `OnEpisodeBegin`):

```csharp
    /// <summary>
    /// Called by TagArenaManager after every reset. Reads the shaping env-params and
    /// seeds Φ from the fresh spawn so the first PBS delta is well-defined.
    /// </summary>
    public void OnArenaReset()
    {
        distanceShapingCoef = Academy.Instance.EnvironmentParameters
            .GetWithDefault("distance_shaping_coef", 0f);
        shapingGamma = Academy.Instance.EnvironmentParameters
            .GetWithDefault("shaping_gamma", shapingGamma);

        if (!shapingParamsLogged)
        {
            Debug.Log($"[TagAgent] distance_shaping_coef={distanceShapingCoef:F2}, " +
                      $"shaping_gamma={shapingGamma:F3}");
            shapingParamsLogged = true;
        }

        prevPotential = CurrentPotential();
    }
```

- [ ] **Step 4: Verify it compiles**

Return to the Unity Editor and check the Console.
Expected: errors about `arena.GetNearestOpponent` and `arena.AllActiveAgents` not existing — those are added in Task 5. This is the expected intermediate state; proceed.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/TagAgent.cs
git commit -m "feat: BufferSensor observations; agent no longer drives arena clock or reset"
```

---

## Task 5: TagArenaManager — lists, clock, reset, registration

**Files:**
- Modify: `Assets/Scripts/TagArenaManager.cs`

- [ ] **Step 1: Replace the fields and Start()**

Replace everything from `[Header("Agent References")]` down to the end of the `Start()` method with:

```csharp
    [Header("Team")]
    public TeamManager teams;   // drag the TagArena prefab's TeamManager here

    [Header("Obstacles (optional — leave empty for the legacy open arena)")]
    public ObstacleManager obstacles;

    [Header("Arena Settings")]
    public float arenaRadius = 8f;
    public float spawnY      = 0.5f;

    [Header("Spawn Safety")]
    public float minSpawnDistance = 3f;
    public int   spawnRetryLimit  = 30;

    [Header("Stalemate Prevention")]
    public int maxEpisodeSteps = 2000;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────
    private int  stepCount    = 0;
    private bool episodeEnded = false;

    private readonly List<TagAgent> activeChasers = new List<TagAgent>();
    private readonly List<TagAgent> activeRunners = new List<TagAgent>();
    private readonly List<Vector2>  spawnBuffer   = new List<Vector2>();

    private int runnersCaughtThisEpisode = 0;

    private StatsRecorder stats;
    private System.Random spawnRng;

    private SimpleMultiAgentGroup chaserGroup;
    private SimpleMultiAgentGroup runnerGroup;

    private void Start()
    {
        // Seeded from UnityEngine.Random, which ML-Agents seeds from --seed, so spawns
        // are reproducible per seed. Same pattern as ObstacleManager.
        spawnRng = new System.Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        chaserGroup = new SimpleMultiAgentGroup();
        runnerGroup = new SimpleMultiAgentGroup();

        stats = Academy.Instance.StatsRecorder;

        // Reset via the Academy hook, NOT a direct call. Environment parameters are not
        // guaranteed to have arrived from the trainer by Start(); reading num_chasers too
        // early would silently run the first episode at the 1v1 default AND latch the
        // wrong values into TeamManager's one-shot log line, breaking smoke criterion 1.
        Academy.Instance.OnEnvironmentReset += ResetArena;
    }
```

> **Why `OnEnvironmentReset` rather than calling `ResetArena()` directly:** the legacy code
> reached `ResetArena` through the chaser's `OnEpisodeBegin`, which fires after the Academy has
> received env-params. Moving reset into the manager loses that ordering guarantee. The Academy
> hook restores it — it fires once at environment initialization, after parameters are set.
> Per-episode resets are still invoked explicitly at the end of `OnAgentTagged` and
> `TriggerStalemate` (Task 6).

Add these using directives at the top of the file if not present:

```csharp
using System.Collections.Generic;
```

- [ ] **Step 2: Add the FixedUpdate clock and remove the old Step()**

Delete the entire `public void Step()` method and replace it with:

```csharp
    // ─────────────────────────────────────────────
    // STEP CLOCK — owned by the arena, ticked once per physics step.
    // Previously the chaser called arena.Step() from OnActionReceived, which does not
    // generalize (N chasers would tick N times) and coupled the clock to agent code.
    // ─────────────────────────────────────────────
    private void FixedUpdate()
    {
        if (episodeEnded) return;

        stepCount++;
        if (stepCount >= maxEpisodeSteps)
            TriggerStalemate();
    }
```

- [ ] **Step 3: Replace ResetArena**

Replace the whole `ResetArena` method and the `SampleSpawn` helper with:

```csharp
    /// <summary>
    /// Full arena reset. Called from Start() and after every episode end — by the arena
    /// itself, never from an agent's OnEpisodeBegin (that was the old ordering bug).
    /// </summary>
    public void ResetArena()
    {
        episodeEnded = false;
        stepCount    = 0;
        runnersCaughtThisEpisode = 0;

        // 1. Obstacles first — spawn rejection needs their new positions.
        if (obstacles != null) obstacles.ResetObstacles();

        // 2. Team sizes from env-params, activating the agents.
        teams.ApplyTeamSizes();

        // 3. Rebuild the active lists from what TeamManager just activated.
        activeChasers.Clear();
        activeRunners.Clear();
        for (int i = 0; i < teams.ActiveChasers; i++) activeChasers.Add(teams.chasers[i]);
        for (int i = 0; i < teams.ActiveRunners; i++) activeRunners.Add(teams.runners[i]);

        // 4. Sample spawns for everyone at once.
        bool ok = SpawnPlacement.TrySampleSpawns(
            activeChasers.Count, activeRunners.Count, arenaRadius, minSpawnDistance,
            ObstaclePositions(), ObstaclePositionCount(), ObstacleClearance(),
            spawnRng, spawnBuffer);

        if (!ok)
        {
            // Fallback: relax separation rather than break training. Logged once so an
            // over-crowded composition is visible in the player log instead of silent.
            Debug.LogWarning($"[TagArenaManager] spawn sampling failed for " +
                             $"{activeChasers.Count}v{activeRunners.Count}; retrying with half separation.");
            SpawnPlacement.TrySampleSpawns(
                activeChasers.Count, activeRunners.Count, arenaRadius, minSpawnDistance * 0.5f,
                ObstaclePositions(), ObstaclePositionCount(), ObstacleClearance(),
                spawnRng, spawnBuffer);
        }

        // 5. Place agents, zero physics, randomize yaw.
        for (int i = 0; i < activeChasers.Count; i++)
            PlaceAgent(activeChasers[i], spawnBuffer[i]);
        for (int i = 0; i < activeRunners.Count; i++)
            PlaceAgent(activeRunners[i], spawnBuffer[activeChasers.Count + i]);

        Physics.SyncTransforms();

        // 6. RE-REGISTER every active agent. SetActive(false) auto-unregistered the ones
        //    caught last episode (SimpleMultiAgentGroup subscribes OnAgentDisabled), so
        //    without this the groups silently drain to empty — with no error raised.
        //    RegisterAgent is idempotent, so re-registering survivors is harmless.
        foreach (TagAgent c in activeChasers) chaserGroup.RegisterAgent(c);
        foreach (TagAgent r in activeRunners) runnerGroup.RegisterAgent(r);

        // 7. Seed per-episode shaping state now that positions are final.
        foreach (TagAgent c in activeChasers) c.OnArenaReset();
        foreach (TagAgent r in activeRunners) r.OnArenaReset();
    }

    private void PlaceAgent(TagAgent agent, Vector2 xz)
    {
        agent.transform.localPosition = new Vector3(xz.x, spawnY, xz.y);
        agent.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        var arb = agent.GetComponent<Rigidbody>();
        arb.linearVelocity  = Vector3.zero;
        arb.angularVelocity = Vector3.zero;
    }

    private static readonly List<Vector2> emptyObstacles = new List<Vector2>();
    private IReadOnlyList<Vector2> ObstaclePositions() =>
        obstacles != null ? obstacles.ActivePositions : emptyObstacles;
    private int ObstaclePositionCount() =>
        obstacles != null ? obstacles.ActivePositions.Count : 0;
    private float ObstacleClearance() =>
        obstacles != null ? obstacles.agentClearance : 0f;
```

- [ ] **Step 4: Expose obstacle positions**

`ObstacleManager` currently keeps `positions` private. In `Assets/Scripts/ObstacleManager.cs`, add this property immediately after the `private readonly List<Vector2> positions` field declaration:

```csharp
    /// <summary>Active obstacle XZ positions — read by TagArenaManager for spawn rejection.</summary>
    public IReadOnlyList<Vector2> ActivePositions => positions;
```

- [ ] **Step 5: Add the agent-query helpers**

Replace the old `GetOpponent` method with:

```csharp
    /// <summary>Nearest ACTIVE opponent to the given agent, or null if none remain.</summary>
    public TagAgent GetNearestOpponent(TagAgent agent)
    {
        List<TagAgent> opponents = (agent.teamId == 0) ? activeRunners : activeChasers;
        TagAgent best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < opponents.Count; i++)
        {
            if (!opponents[i].gameObject.activeInHierarchy) continue;
            float d = (opponents[i].transform.localPosition - agent.transform.localPosition).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = opponents[i]; }
        }
        return best;
    }

    /// <summary>Every currently-active agent in this arena, both teams.</summary>
    public IEnumerable<TagAgent> AllActiveAgents()
    {
        for (int i = 0; i < activeChasers.Count; i++)
            if (activeChasers[i].gameObject.activeInHierarchy) yield return activeChasers[i];
        for (int i = 0; i < activeRunners.Count; i++)
            if (activeRunners[i].gameObject.activeInHierarchy) yield return activeRunners[i];
    }
```

- [ ] **Step 6: Verify it compiles**

Unity Editor Console.
Expected: errors only in `OnAgentTagged`/`TriggerStalemate` referencing the removed `chaser`/`runner` fields — fixed in Task 6.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/TagArenaManager.cs Assets/Scripts/ObstacleManager.cs
git commit -m "refactor: arena owns step clock and reset; agent lists replace single refs"
```

---

## Task 6: Termination and reward delivery

**Files:**
- Modify: `Assets/Scripts/TagArenaManager.cs`

- [ ] **Step 1: Replace TriggerStalemate**

```csharp
    /// <summary>
    /// Time ran out. Every runner still alive scores a survival share for its team.
    /// Truncation, not a true terminal ⇒ GroupEpisodeInterrupted bootstraps the value.
    /// </summary>
    private void TriggerStalemate()
    {
        if (episodeEnded) return;
        episodeEnded = true;

        int n = activeRunners.Count;
        int survivors = 0;
        foreach (TagAgent r in activeRunners)
            if (r.gameObject.activeInHierarchy) survivors++;

        for (int i = 0; i < survivors; i++)
        {
            chaserGroup.AddGroupReward(TagReward.SurvivalShareChaser(n));
            runnerGroup.AddGroupReward(TagReward.SurvivalShareRunner(n));
            if (IndividualTerminalRewardOn())
            {
                foreach (TagAgent c in activeChasers)
                    if (c.gameObject.activeInHierarchy) c.AddReward(TagReward.SurvivalShareChaser(n));
                foreach (TagAgent r in activeRunners)
                    if (r.gameObject.activeInHierarchy) r.AddReward(TagReward.SurvivalShareRunner(n));
            }
        }

        RecordEpisodeStats(allCaught: false);

        chaserGroup.GroupEpisodeInterrupted();
        runnerGroup.GroupEpisodeInterrupted();
        ResetArena();
    }
```

- [ ] **Step 2: Replace OnAgentTagged**

```csharp
    /// <summary>
    /// Called from TagAgent.OnCollisionEnter. A catch is scored chaser-side regardless of
    /// which collider fired. The tagged runner is rewarded BEFORE deactivation — order is
    /// load-bearing, because SetActive(false) auto-unregisters it from its group and it can
    /// receive nothing afterwards.
    /// </summary>
    public void OnAgentTagged(TagAgent a, TagAgent b)
    {
        if (episodeEnded) return;

        TagAgent runner = (a.teamId == 1) ? a : (b.teamId == 1 ? b : null);
        if (runner == null) return;                       // chaser-chaser bump
        if (a.teamId == b.teamId) return;                 // same-team bump
        if (!runner.gameObject.activeInHierarchy) return;  // already caught this episode

        int n = activeRunners.Count;

        chaserGroup.AddGroupReward(TagReward.CatchShareChaser(stepCount, maxEpisodeSteps, n));
        runnerGroup.AddGroupReward(TagReward.CatchShareRunner(stepCount, maxEpisodeSteps, n));

        if (IndividualTerminalRewardOn())
        {
            // PPO arm: mirror to every STILL-ACTIVE agent (the tagged runner included —
            // it is still active at this point). Agents deactivated earlier miss this,
            // which is exactly the limitation under test.
            foreach (TagAgent c in activeChasers)
                if (c.gameObject.activeInHierarchy)
                    c.AddReward(TagReward.CatchShareChaser(stepCount, maxEpisodeSteps, n));
            foreach (TagAgent r in activeRunners)
                if (r.gameObject.activeInHierarchy)
                    r.AddReward(TagReward.CatchShareRunner(stepCount, maxEpisodeSteps, n));
        }

        stats.Add("Environment/TimeToCatch", stepCount);
        runnersCaughtThisEpisode++;

        runner.gameObject.SetActive(false);   // AFTER the rewards above

        // Episode ends only when every runner has been caught.
        if (runnersCaughtThisEpisode >= n)
        {
            episodeEnded = true;
            RecordEpisodeStats(allCaught: true);
            chaserGroup.EndGroupEpisode();
            runnerGroup.EndGroupEpisode();
            ResetArena();
        }
    }
```

- [ ] **Step 3: Add the stats helper**

```csharp
    /// <summary>
    /// Episode outcome metrics. Recorded BEFORE the group-end calls, matching the fix that
    /// resolved the old TimeToCatch = 0 bug.
    ///   Environment/Catch            1 = every runner caught (STRICTER than the 1v1 metric)
    ///   Environment/RunnersSurvived  fraction alive at episode end — the primary cross-arm
    ///                                metric for Phase C, since ELO cannot carry comparisons.
    /// </summary>
    private void RecordEpisodeStats(bool allCaught)
    {
        int n = activeRunners.Count;
        stats.Add("Environment/Catch", allCaught ? 1f : 0f);
        stats.Add("Environment/RunnersSurvived", n > 0 ? (float)(n - runnersCaughtThisEpisode) / n : 0f);
    }
```

- [ ] **Step 4: Verify it compiles and run the EditMode tests**

Unity Editor Console: expected no errors.
Test Runner → `EditMode` → `Run All`. Expected: **28 tests pass**.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/TagArenaManager.cs
git commit -m "feat: per-event team reward delivery, staged runner removal, RunnersSurvived stat"
```

---

## Task 7 [USER]: Prefab and scene authoring

**Files:**
- Modify: `Assets/Prefabs/TagArena.prefab`, `Assets/Prefabs/ChaserAgent.prefab`, `Assets/Prefabs/RunnerAgent.prefab`

This task is Unity Editor work and cannot be scripted reliably. Hand the user this checklist.

- [ ] **Step 1: Add BufferSensorComponent to both agent prefabs**

Open `Assets/Prefabs/ChaserAgent.prefab`. `Add Component` → `Buffer Sensor Component`. Set:
- `Sensor Name`: `EntitySensor`
- `Observable Size`: **10**
- `Max Num Observables`: **7**

Repeat identically for `Assets/Prefabs/RunnerAgent.prefab`.

- [ ] **Step 2: Populate TagArena with 4+4 agents**

Open `Assets/Prefabs/TagArena.prefab`. It currently holds one ChaserAgent and one RunnerAgent.
- Duplicate the ChaserAgent instance 3 times → 4 total.
- Duplicate the RunnerAgent instance 3 times → 4 total.
- Set instances 2, 3, 4 of **each** to **inactive** (uncheck the box beside the name in the Inspector). Instance 1 of each stays active.

- [ ] **Step 3: Add and wire TeamManager**

On the TagArena prefab root, `Add Component` → `Team Manager`. Then:
- Drag all 4 chaser instances into `Chasers` (size 4).
- Drag all 4 runner instances into `Runners` (size 4).
- On the existing `TagArenaManager` component, drag the TagArena root into the new `Teams` field.

- [ ] **Step 4: Verify the agent count in the scene**

Open `Assets/Scenes/Scene_V2.unity`. It holds 16 TagArena instances; all inherit the prefab change automatically. Confirm the Hierarchy shows 8 agents per arena with 6 inactive.

- [ ] **Step 5: Commit**

```bash
git add Assets/Prefabs/TagArena.prefab Assets/Prefabs/ChaserAgent.prefab Assets/Prefabs/RunnerAgent.prefab Assets/Scenes/Scene_V2.unity
git commit -m "chore: TagArena carries 4+4 agents with TeamManager; agents gain BufferSensor"
```

---

## Task 8: Phase C trainer configs

**Files:**
- Create: `experiments/gen_team_configs.py`

- [ ] **Step 1: Write the generator**

Create `experiments/gen_team_configs.py`:

```python
# -*- coding: utf-8 -*-
"""Generates Phase C trainer configs into BOTH the ml-agents config dir and the
repo archive, mirroring experiments/gen_gamma_configs.py.

Diffs vs TagMApoca_sparse_5M.yaml are ONLY: trainer_type (poca|ppo), the
individual_terminal_reward env-param (PPO arm), and num_chasers/num_runners.
"""
import io, os

MLAGENTS_CFG = r"C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca"
ARCHIVE      = r"c:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\experiments\configs"

COMPOSITIONS = {"2v2": (2, 2), "3v3": (3, 3)}


def behavior(name, trainer, max_steps, summary_freq, ckpt):
    return f"""  {name}:
    trainer_type: {trainer}
    hyperparameters:
      batch_size: 2048
      buffer_size: 40960
      learning_rate: 3.0e-4
      learning_rate_schedule: linear
      beta: 5.0e-3
      beta_schedule: constant
      epsilon: 0.2
      epsilon_schedule: linear
      lambd: 0.95
      num_epoch: 5
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 2
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: {max_steps}
    time_horizon: 256
    summary_freq: {summary_freq}
    checkpoint_interval: {ckpt}
    keep_checkpoints: 20
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 50000
      swap_steps: 50000
      team_change: 100000
      initial_elo: 1200.0
"""


def config(header, trainer, nc, nr, indiv_term, smoke=False):
    if smoke:
        max_steps, summary, ckpt = 50000, 10000, 25000
    else:
        max_steps, summary, ckpt = 5000000, 50000, 250000
    body = header + "behaviors:\n"
    body += behavior("Chaser", trainer, max_steps, summary, ckpt)
    body += behavior("Runner", trainer, max_steps, summary, ckpt)
    body += f"""
environment_parameters:
  distance_shaping_coef: 0.0
  shaping_gamma: 0.99
  num_obstacles: 4
  obstacle_layout: 1
  num_chasers: {nc}
  num_runners: {nr}
  individual_terminal_reward: {indiv_term}
"""
    return body


FILES = {}

for tag, (nc, nr) in COMPOSITIONS.items():
    FILES[f"TagMApoca_team_{tag}_poca.yaml"] = config(
        f"# PHASE C (RQ-D) — MA-POCA, {tag}, sparse, 4 randomized pillars, gamma 0.99.\n"
        f"# Terminal reward flows through the GROUP channel only.\n",
        "poca", nc, nr, "0.0")
    FILES[f"TagMApoca_team_{tag}_ppo.yaml"] = config(
        f"# PHASE C (RQ-D) — PPO baseline, {tag}, sparse, 4 randomized pillars, gamma 0.99.\n"
        f"# PPO ignores group rewards, so the shared team reward is mirrored individually\n"
        f"# via individual_terminal_reward. Agents deactivated earlier miss later events —\n"
        f"# that is the documented limitation under test, not an implementation shortcut.\n",
        "ppo", nc, nr, "1.0")

FILES["TagMApoca_team_smoke.yaml"] = config(
    "# SMOKE GATE for the Phase C binary: 50k, MA-POCA, 2v2, randomized pillars.\n"
    "# Gate criterion 3: Losses/Baseline Loss / Losses/Value Loss must exceed 1.05.\n",
    "poca", 2, 2, "0.0", smoke=True)

for target in (MLAGENTS_CFG, ARCHIVE):
    os.makedirs(target, exist_ok=True)
    for name, text in FILES.items():
        with io.open(os.path.join(target, name), "w", encoding="utf8", newline="\n") as f:
            f.write(text)
        print("wrote", os.path.join(target, name))
```

- [ ] **Step 2: Run it**

```bash
python experiments/gen_team_configs.py
```
Expected: 10 lines of `wrote ...` — 5 files × 2 targets.

- [ ] **Step 3: Verify the YAML parses and carries the right params**

```bash
python -c "import yaml,io; d=yaml.safe_load(io.open(r'C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\TagMApoca_team_2v2_ppo.yaml',encoding='utf8')); print(d['behaviors']['Chaser']['trainer_type']); print(d['environment_parameters'])"
```
Expected output:
```
ppo
{'distance_shaping_coef': 0.0, 'shaping_gamma': 0.99, 'num_obstacles': 4, 'obstacle_layout': 1, 'num_chasers': 2, 'num_runners': 2, 'individual_terminal_reward': 1.0}
```

- [ ] **Step 4: Commit**

```bash
git add experiments/gen_team_configs.py experiments/configs/TagMApoca_team_*.yaml
git commit -m "feat: Phase C trainer configs for POCA and PPO team arms"
```

---

## Task 9 [USER]: Rebuild and throughput bake-off

- [ ] **Step 1: Rebuild the headless player**

Unity: `File > Build Settings` → confirm only `Scene_V2` is ticked, platform Windows x86_64 → `Build` → overwrite
`C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe`.

Verify afterwards that `Build/TagMApoca_V1_Data/Managed/Assembly-CSharp.dll` is dated today.

- [ ] **Step 2: Measure throughput at 2v2**

In the Anaconda Prompt:
```
conda activate mlagents
cd C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
mlagents-learn config\poca\TagMApoca_team_smoke.yaml --env="C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe" --no-graphics --run-id=TeamBake_2v2 --seed 1
```

- [ ] **Step 3: Record the rate**

From the run's console output, note the steps/sec reported in the periodic `[INFO]` lines.
Compute: `hours_per_5M_run = 5_000_000 / steps_per_sec / 3600`.

Reference point: Phase B measured **~4.3 h per 5M run** at 16 arenas × 2 agents.

Report the measured number — it feeds the matrix decision in Task 12.

---

## Task 10 [USER]: Smoke gate

- [ ] **Step 1: Reuse the bake-off run, or re-run if it was interrupted**

The `TeamBake_2v2` run from Task 9 doubles as the smoke run. If it completed 50k, use it.

- [ ] **Step 2: Verify all six criteria (Claude checks with the user)**

Run this from the project root:

```bash
python - <<'EOF'
import sys, os, glob, math
sys.path.insert(0, r"c:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\experiments\analysis")
from parse_tb import extract_scalars
R   = r"C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\results"
RUN = "TeamBake_2v2"
for beh in ("Chaser", "Runner"):
    m = {}
    for f in sorted(glob.glob(os.path.join(R, RUN, beh, "events.out.tfevents.*"))):
        for t, p in extract_scalars(f).items(): m.setdefault(t, []).extend(p)
    def l5(tag):
        p = sorted(m.get(tag, []))
        return sum(v for _, v in p[-5:]) / len(p[-5:]) if p else float('nan')
    b, v = l5("Losses/Baseline Loss"), l5("Losses/Value Loss")
    nonfin = sum(1 for t, pts in m.items() for _, x in pts if math.isnan(x) or math.isinf(x))
    print(f"== {RUN}/{beh}")
    print(f"   Baseline/Value ratio : {b/v:.4f}   <-- CRITERION 3: must exceed 1.05")
    print(f"   Catch                : {l5('Environment/Catch'):.4f}")
    print(f"   RunnersSurvived      : {l5('Environment/RunnersSurvived'):.4f}")
    print(f"   non-finite values    : {nonfin}")
EOF
```

Then check the Player log:
```bash
grep -E "TeamManager|ObstacleManager|TagAgent\]|spawn sampling failed" "C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results/TeamBake_2v2/run_logs/Player-0.log"
```

**Gate criteria:**

| # | Criterion | Pass condition |
|---|---|---|
| 1 | Env-params reached the binary | `[TeamManager] num_chasers=2, num_runners=2` present |
| 2 | Clean run | 50k completed both behaviours; `non-finite values = 0`; no Unity errors |
| 3 | **Baseline engages** | **Baseline/Value ratio > 1.05** |
| 4 | ONNX export with BufferSensor | `Chaser.onnx` and `Runner.onnx` exist in the run dir |
| 5 | No group drain | no `spawn sampling failed` warnings; `Environment/Catch` > 0 |
| 6 | Team metrics recorded | `RunnersSurvived` present and strictly between 0 and 1 |

**Criterion 3 is the phase's premise.** If the ratio is still ~1.00–1.01, stop and invoke the pre-committed fallback in the spec: report "the baseline is inert even at N>1" as the finding and pivot the budget to the demonstration claim. Do not spend 5M-step runs on a dead hypothesis.

**Criterion 4 fallback:** if ONNX export fails on the attention graph, replace the `BufferSensorComponent` with 7 fixed padded slots in the vector observation (Task 4's design, minus permutation invariance) and re-smoke.

---

## Task 11 [USER]: 1v1 regression run

- [ ] **Step 1: Launch**

```
mlagents-learn config\poca\TagMApoca_sparse_obsR_g099.yaml --env="C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe" --no-graphics --run-id=POCA_regress_1v1_s1 --seed 1
```

This config sets no `num_chasers`/`num_runners`, so both default to 1 — which is the point: the exact config that produced Phase B must still produce Phase B behaviour.

Duration ~4.3 h.

- [ ] **Step 2: Compare against Phase B's 3-seed band**

```bash
cd "c:/Users/david/Documents/PROGRAMMING/UnityProjects/TagMApoca_V1"
python experiments/analysis/parse_tb.py \
  "C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results/POCA_regress_1v1_s1" \
  "C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results/POCA_sparse_obsR_g099_s1" \
  "C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results/POCA_sparse_obsR_g099_s2" \
  "C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results/POCA_sparse_obsR_g099_s3"
```

**Pass condition:** the regression run's Chaser `Environment/Catch` and ELO gap land inside Phase B's band — catch 0.999 (range 0.001), ELO gap 1257 (range 34). Episode length ~47.

**If it lands outside:** this is informative, not fatal. The refactor fixed the terminal-observation bug described in the spec §3 (the runner's terminal observation was previously its next-episode spawn). A shift in *runner* metrics specifically would be evidence that bug was costing something — a reportable methods finding. Record the deviation with its magnitude; do not silently proceed.

---

## Task 12: Matrix decision and batch script

**Files:**
- Create: `experiments/run_phaseC.bat`

- [ ] **Step 1: Choose the matrix with the user**

Inputs: measured hours/run from Task 9, and the maximum feasible composition from Task 2's feasibility test.

Default recommendation, assuming ~8 h/run at 2v2:

| Arm | Run-ids | Runs |
|---|---|---|
| MA-POCA 2v2 | `POCA_team_2v2_s1/s2/s3` | 3 |
| PPO 2v2 | `PPO_team_2v2_s1/s2/s3` | 3 |

~48 h total. If the budget must shrink, drop to 2 seeds per arm — **never below 2**, and never to a single composition with 1 seed each.

- [ ] **Step 2: Write the batch**

Create `experiments/run_phaseC.bat` (CRLF line endings):

```bat
@echo off
setlocal
REM ============================================================================
REM  PHASE C (RQ-D) - MA-POCA vs PPO at 2v2. Sparse, gamma 0.99, random pillars.
REM  PREREQ: Phase C binary rebuilt + TagMApoca_team_smoke gate PASSED,
REM          especially criterion 3 (Baseline/Value ratio > 1.05).
REM  Run from the Anaconda Prompt (conda env "mlagents").
REM  POCA arm runs first: if interrupted, the primary arm is complete.
REM ============================================================================

set "ENV=C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe"
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
set "CFG=config\poca"

if not exist "%ENV%" (
  echo [ERROR] Headless build not found at "%ENV%".
  pause
  exit /b 1
)
if not exist batch_logs mkdir batch_logs

echo Starting Phase C at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s1 --seed 1 > batch_logs\POCA_team_2v2_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s2 --seed 2 > batch_logs\POCA_team_2v2_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_poca.yaml --env="%ENV%" --no-graphics --run-id=POCA_team_2v2_s3 --seed 3 > batch_logs\POCA_team_2v2_s3.log 2>&1

mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s1  --seed 1 > batch_logs\PPO_team_2v2_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s2  --seed 2 > batch_logs\PPO_team_2v2_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_team_2v2_ppo.yaml  --env="%ENV%" --no-graphics --run-id=PPO_team_2v2_s3  --seed 3 > batch_logs\PPO_team_2v2_s3.log 2>&1

echo Phase C complete at %DATE% %TIME%
endlocal
pause
```

- [ ] **Step 3: Commit**

```bash
git add experiments/run_phaseC.bat
git commit -m "feat: Phase C batch script - POCA vs PPO at 2v2, 3 seeds each"
```

---

## Task 13: Analysis and write-up

**Files:**
- Create: `experiments/analysis/analyze_phaseC.py`
- Modify: `docs/Theory.md`, `docs/progress.md`, `CLAUDE.md`

- [ ] **Step 1: Write the analysis script**

Create `experiments/analysis/analyze_phaseC.py`:

```python
# -*- coding: utf-8 -*-
"""Phase C analysis: MA-POCA vs PPO at team sizes > 1.

Reports the pre-registered quantities from the spec's section 5:
  P1  Baseline/Value ratio departs from unity  (POCA arm only; PPO has no baseline)
  P2  effect asymmetry: runner-side vs chaser-side
  P3  runner survival fraction, POCA vs PPO
"""
import sys, os, glob, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from parse_tb import extract_scalars

R = r"C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\results"
ARMS = {
    "MA-POCA 2v2": ["POCA_team_2v2_s1", "POCA_team_2v2_s2", "POCA_team_2v2_s3"],
    "PPO 2v2":     ["PPO_team_2v2_s1",  "PPO_team_2v2_s2",  "PPO_team_2v2_s3"],
}
TAGS = ["Environment/Catch", "Environment/RunnersSurvived",
        "Environment/Episode Length", "Self-play/ELO"]


def last5(run, beh, tag):
    m = {}
    for f in sorted(glob.glob(os.path.join(R, run, beh, "events.out.tfevents.*"))):
        for t, p in extract_scalars(f).items():
            m.setdefault(t, []).extend(p)
    pts = sorted(m.get(tag, []))
    if not pts:
        return float('nan')
    return sum(v for _, v in pts[-5:]) / len(pts[-5:])


def agg(runs, beh, tag):
    xs = [last5(r, beh, tag) for r in runs]
    xs = [x for x in xs if not math.isnan(x)]
    if not xs:
        return float('nan'), float('nan')
    return sum(xs) / len(xs), max(xs) - min(xs)


print("=" * 78)
print("P1 - Baseline/Value ratio (POCA only; PPO logs no Baseline Loss)")
for run in ARMS["MA-POCA 2v2"]:
    b = last5(run, "Chaser", "Losses/Baseline Loss")
    v = last5(run, "Chaser", "Losses/Value Loss")
    verdict = "ENGAGED" if (b / v) > 1.05 else "INERT"
    print(f"  {run:<22} ratio {b/v:.4f}   {verdict}")
ppo_b = last5(ARMS["PPO 2v2"][0], "Chaser", "Losses/Baseline Loss")
print(f"  PPO_team_2v2_s1        Baseline Loss present: {not math.isnan(ppo_b)} (expected False)")

print()
print("=" * 78)
print("P3 - primary cross-arm metric: runner survival fraction")
for arm, runs in ARMS.items():
    m, r = agg(runs, "Runner", "Environment/RunnersSurvived")
    print(f"  {arm:<14} RunnersSurvived {m:.4f}  (range {r:.4f})")

print()
print("=" * 78)
print("P2 - effect asymmetry: runner-side should separate more than chaser-side")
for tag in TAGS:
    print(f"  {tag}")
    for beh in ("Chaser", "Runner"):
        row = []
        for arm, runs in ARMS.items():
            m, rng = agg(runs, beh, tag)
            row.append(f"{arm} {m:9.3f} (r {rng:.3f})")
        print(f"    {beh:<8}" + "   ".join(row))
```

- [ ] **Step 2: Run it**

```bash
python experiments/analysis/analyze_phaseC.py
```
Expected: three labelled blocks. Every `MA-POCA` row under P1 should read `ENGAGED`; the PPO line should report `Baseline Loss present: False`, confirming the arms really are different algorithms.

- [ ] **Step 3: Write Theory.md section 16**

Add a new `## 16. Phase C — MA-POCA vs PPO at team sizes > 1 (written YYYY-MM-DD)` after section 15. It must contain, in this order:

1. Run validity table (all runs 5M, ONNX exported, non-finite counts, `[TeamManager]` log lines)
2. Per-seed results table for both arms: Catch, RunnersSurvived, Episode Length, ELO gap, Baseline/Value ratio
3. A subsection per pre-registered prediction P1–P4 stating **confirmed / falsified / partial** explicitly
4. Caveats, including: `Environment/Catch` at N>1 means "all runners caught" and is therefore **not** comparable to the Phase A/B values without saying so; ELO cannot carry cross-arm comparisons; and the observation-spec caveat (1v1 values identical but the network gained an attention encoder)
5. A closing verdict answering "why MA-POCA at all?" — including, if P3 is falsified, a plain statement that the equivalence found at 1v1 extends to teams

- [ ] **Step 4: Update the session log and project recap**

Add a dated entry to `docs/progress.md` (newest first) and a dated block to `CLAUDE.md`'s Core Code Reference summarizing Phase C's outcome and the new env-params.

- [ ] **Step 5: Commit**

```bash
git add experiments/analysis/analyze_phaseC.py docs/Theory.md docs/progress.md CLAUDE.md
git commit -m "results: Phase C - MA-POCA vs PPO at 2v2"
```

- [ ] **Step 6: Finish the branch**

Invoke `superpowers:finishing-a-development-branch`. **Do not merge to main without the user's explicit approval.**

---

## Self-review

**Spec coverage:**

| Spec section | Covered by |
|---|---|
| §2 D1 runner deactivates, episode continues | Task 6 |
| §2 D2 BufferSensor observations | Task 4, Task 7 |
| §2 D3 Approach A generalize in place | Tasks 4–6 |
| §2 D5 PPO shared reward, departed agents miss | Task 6 Step 2, Task 8 |
| §2 D6 fixed team sizes | Task 3, Task 8 |
| §3 architecture, five components | Tasks 1–6 |
| §3 registration trap | Task 5 Step 3 item 6 |
| §3 env-params defaulting to 1 | Task 3, verified Task 11 |
| §3 gotcha #3 fix | Task 4 Step 2, Task 5 Step 2 |
| §4 reward structure | Task 1, Task 6 |
| §5 P1–P4 pre-registered | Task 10 criterion 3, Task 13 |
| §6 seven stages | Tasks 9–13 |
| §7 testing | Tasks 1–2 |
| §8 new stats | Task 6 Step 3 |

**Placeholder scan:** no TBD/TODO. Task 12's run count is a decision point with a stated default and a stated floor, not a placeholder. Task 13 Step 3's date is filled at execution time.

**Type consistency check:** `TagReward.{TimeBonus, SurvivalBonus, CatchShareChaser, CatchShareRunner, SurvivalShareChaser, SurvivalShareRunner}` defined Task 1, used Task 6. `SpawnPlacement.TrySampleSpawns` defined Task 2, called Task 5 with matching argument order. `TeamManager.{chasers, runners, ActiveChasers, ActiveRunners, ApplyTeamSizes}` defined Task 3, used Task 5. `TagAgent.OnArenaReset` defined Task 4, called Task 5. `TagArenaManager.{GetNearestOpponent, AllActiveAgents}` defined Task 5, called Task 4. `ObstacleManager.ActivePositions` added Task 5 Step 4, used Task 5 Step 3.

**One knowing inconsistency:** Task 4 is committed in a non-compiling state, since it calls methods added in Task 5. This is flagged in Task 4 Step 4 and is the natural seam for an in-place refactor of two mutually-referencing classes. The tree compiles again at the end of Task 5.
