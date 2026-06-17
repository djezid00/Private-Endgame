# Chaser Reward Shaping (Sparse-vs-Shaped Experiment) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add config-driven potential-based reward shaping for the chaser and the metrics to evaluate it, so we can run a sparse-vs-shaped comparison as one clean experiment.

**Architecture:** The PBS math is extracted into a pure static `TagReward` class in its own assembly (`TagGame.Reward`) so it can be unit-tested in isolation — this is the only part that is genuinely unit-testable, and a sign/normalisation bug here would silently corrupt the shaped arm. `TagAgent` reads a per-episode `distance_shaping_coef` from ML-Agents `environment_parameters` (0 in the sparse arm, 0.5 in the shaped arm) and applies the telescoping PBS reward to the chaser only. `TagArenaManager` logs `catch` and `time_to_catch` via `StatsRecorder` so catch-rate and time-to-catch appear as TensorBoard scalars. Runtime/training behaviour is verified empirically (console + TensorBoard), not by unit tests.

**Tech Stack:** Unity 6, C#, Unity ML-Agents (poca trainer, self-play), Unity Test Framework (NUnit, EditMode), conda `mlagents` env (run from the Anaconda Prompt only).

**Spec:** `docs/superpowers/specs/2026-06-17-chaser-reward-shaping-design.md`

**Note on two repos:** C# code lives in this Unity project (git: `Private-Endgame`). The trainer YAML configs live in the separate ML-Agents repo at `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\` (not this git). A copy of each config is archived into this repo under `experiments/configs/` for thesis reproducibility.

**Constants used throughout:**
- `arenaDiagonal = 28.28f` — max chaser↔runner planar distance on the 20×20 floor (√(20²+20²)), used to normalise Φ into [−coef, 0].
- `shapingGamma = 0.99f` — must equal the trainer's `extrinsic.gamma`.
- Shaped arm `distance_shaping_coef = 0.5`; sparse arm `0.0`.

---

### Task 1: Pure PBS reward math + EditMode unit tests

**Files:**
- Create: `Assets/Scripts/Reward/TagReward.cs`
- Create: `Assets/Scripts/Reward/TagGame.Reward.asmdef`
- Create: `Assets/Tests/EditMode/TagGame.EditTests.asmdef`
- Create: `Assets/Tests/EditMode/TagRewardTests.cs`

- [ ] **Step 1: Create the runtime assembly definition**

Create `Assets/Scripts/Reward/TagGame.Reward.asmdef` (its own assembly so a test assembly can reference it; `autoReferenced: true` keeps it usable from `Assembly-CSharp`, i.e. `TagAgent`):

```json
{
    "name": "TagGame.Reward",
    "rootNamespace": "",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Write the failing tests**

Create `Assets/Tests/EditMode/TagGame.EditTests.asmdef`:

```json
{
    "name": "TagGame.EditTests",
    "rootNamespace": "",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "TagGame.Reward"
    ],
    "includePlatforms": [ "Editor" ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [ "nunit.framework.dll" ],
    "autoReferenced": false,
    "defineConstraints": [ "UNITY_INCLUDE_TESTS" ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Create `Assets/Tests/EditMode/TagRewardTests.cs`:

```csharp
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
```

- [ ] **Step 3: Run the tests to verify they FAIL**

In Unity: `Window > General > Test Runner > EditMode > Run All`.
Expected: compile error / all tests fail because `TagReward` does not exist yet.
(CLI alternative, from a normal shell: `"<UnityEditorPath>\Unity.exe" -batchmode -runTests -projectPath "c:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1" -testPlatform EditMode -testResults "%TEMP%\tagreward.xml" -quit`)

- [ ] **Step 4: Implement `TagReward`**

Create `Assets/Scripts/Reward/TagReward.cs`:

```csharp
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
```

- [ ] **Step 5: Run the tests to verify they PASS**

In Unity: `Window > General > Test Runner > EditMode > Run All`.
Expected: 5 passed (PlanarDistance_IgnoresY, Potential_Closer_IsGreaterThan_Farther, Potential_ZeroCoef_IsAlwaysZero, ShapingDelta_Closing_IsPositive, ShapingDelta_Receding_IsNegative).

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Reward Assets/Tests
git commit -m "feat: pure PBS reward math (TagReward) + EditMode unit tests"
```

---

### Task 2: Apply config-driven PBS shaping in TagAgent (chaser only)

**Files:**
- Modify: `Assets/Scripts/TagAgent.cs`

- [ ] **Step 1: Add shaping fields**

In `Assets/Scripts/TagAgent.cs`, after the existing `[Header("Movement")]` block (the `moveSpeed`/`turnSpeed` fields, lines ~17-19), add:

```csharp
    [Header("Reward Shaping (chaser only)")]
    public float arenaDiagonal = 28.28f; // max chaser↔runner planar distance (20x20 floor), normalises Φ
    public float shapingGamma  = 0.99f;  // MUST match trainer extrinsic.gamma

    // Set once per episode from environment_parameters (0 in the sparse arm).
    private float distanceShapingCoef = 0f;
    private float prevPotential       = 0f;
```

- [ ] **Step 2: Add the potential helper**

In the same file, add this private method (e.g. just above `OnActionReceived`):

```csharp
    // Current potential Φ(s) for the chaser, from live positions. Uses localPosition
    // to match the observation frame (both agents are children of the same arena).
    private float CurrentPotential()
    {
        TagAgent opponent = arena.GetOpponent(this);
        return TagReward.Potential(transform.localPosition,
                                   opponent.transform.localPosition,
                                   distanceShapingCoef, arenaDiagonal);
    }
```

- [ ] **Step 3: Read the coefficient and seed the potential in OnEpisodeBegin**

Replace the current body of `OnEpisodeBegin` (lines ~44-50):

```csharp
    public override void OnEpisodeBegin()
    {
        if (teamId == 0)
            arena.ResetArena();

        // Runner does nothing here — it just waits for chaser's reset to place it.
    }
```

with:

```csharp
    public override void OnEpisodeBegin()
    {
        if (teamId == 0)
        {
            arena.ResetArena();

            // Select this arm's shaping coefficient from the trainer config
            // (environment_parameters.distance_shaping_coef). 0 ⇒ sparse arm.
            distanceShapingCoef = Academy.Instance.EnvironmentParameters
                .GetWithDefault("distance_shaping_coef", 0f);

            // Seed Φ from the freshly-reset spawn so the first PBS delta is well-defined.
            prevPotential = CurrentPotential();
        }

        // Runner does nothing here — it just waits for chaser's reset to place it.
    }
```

(`Academy` is in `Unity.MLAgents`, already imported at the top of the file.)

- [ ] **Step 4: Apply the PBS reward in OnActionReceived (chaser branch)**

In `OnActionReceived`, replace the chaser branch (currently lines ~110-120):

```csharp
        if (teamId == 0) // CHASER
        {
            // Small negative reward every step → chaser is punished for wasting time
            // This creates urgency: catch the runner as fast as possible.
            AddReward(-0.001f);

            // Only the chaser ticks the arena step clock.
            // If the runner also called arena.Step(), the timer would advance
            // twice per frame (double-speed stalemate).
            arena.Step();
        }
```

with:

```csharp
        if (teamId == 0) // CHASER
        {
            // Small negative reward every step → chaser is punished for wasting time
            // This creates urgency: catch the runner as fast as possible.
            AddReward(-0.001f);

            // Potential-based shaping: reward closing distance to the runner.
            // Policy-invariant (Ng et al. 1999). No-op in the sparse arm (coef 0 ⇒ Φ ≡ 0).
            float curPotential = CurrentPotential();
            AddReward(TagReward.ShapingDelta(prevPotential, curPotential, shapingGamma));
            prevPotential = curPotential;

            // Only the chaser ticks the arena step clock.
            // If the runner also called arena.Step(), the timer would advance
            // twice per frame (double-speed stalemate).
            arena.Step();
        }
```

- [ ] **Step 5: Verify it compiles with no Console errors**

In Unity: let the editor recompile. Expected: no red errors in the Console; `TagAgent` inspector now shows `Arena Diagonal` (28.28) and `Shaping Gamma` (0.99) fields on both agent prefabs.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/TagAgent.cs
git commit -m "feat: config-driven potential-based shaping for the chaser"
```

---

### Task 3: Log catch / time-to-catch stats from TagArenaManager

**Files:**
- Modify: `Assets/Scripts/TagArenaManager.cs`

- [ ] **Step 1: Add a StatsRecorder field**

In `Assets/Scripts/TagArenaManager.cs`, in the PRIVATE STATE block (after `private Rigidbody runnerRb;`, line ~34) add:

```csharp
    // ML-Agents stats sink — surfaces custom scalars in TensorBoard.
    private StatsRecorder stats;
```

- [ ] **Step 2: Initialise it in Start**

In `Start()`, after the group registration (after `runnerGroup.RegisterAgent(runner);`, line ~64), add:

```csharp
        // StatsRecorder lets us log episode outcomes as TensorBoard scalars.
        stats = Academy.Instance.StatsRecorder;
```

- [ ] **Step 3: Record a stalemate as a non-catch**

In `TriggerStalemate()`, after `runnerGroup.GroupEpisodeInterrupted();` (line ~156) add:

```csharp
        // Outcome metric: 0 = no catch this episode (averaged ⇒ catch rate).
        stats.Add("Environment/Catch", 0f);
```

- [ ] **Step 4: Record a catch and its timing**

In `OnAgentTagged(...)`, after `runnerGroup.EndGroupEpisode();` (line ~199) add:

```csharp
        // Outcome metrics: 1 = catch (averaged ⇒ catch rate); stepCount = time-to-catch
        // (averaged over catches only ⇒ mean steps to catch).
        stats.Add("Environment/Catch", 1f);
        stats.Add("Environment/TimeToCatch", stepCount);
```

- [ ] **Step 5: Verify it compiles with no Console errors**

In Unity: let the editor recompile. Expected: no red errors in the Console.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/TagArenaManager.cs
git commit -m "feat: log catch rate + time-to-catch via StatsRecorder"
```

---

### Task 4: Create the sparse and shaped trainer configs

**Files:**
- Create: `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\TagMApoca_sparse.yaml`
- Create: `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\TagMApoca_shaped.yaml`
- Create (archive copy in this repo): `experiments/configs/TagMApoca_sparse.yaml`
- Create (archive copy in this repo): `experiments/configs/TagMApoca_shaped.yaml`

- [ ] **Step 1: Write the sparse config**

Create `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\TagMApoca_sparse.yaml`:

```yaml
# Validation arm A — SPARSE: terminal ±1 + ±0.001/step only, NO distance shaping.
# 400k-step budget; everything except distance_shaping_coef matches TagMApoca_shaped.yaml.
behaviors:
  Chaser:
    trainer_type: poca
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
    max_steps: 400000
    time_horizon: 256
    summary_freq: 10000
    checkpoint_interval: 100000
    keep_checkpoints: 5
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 25000
      swap_steps: 25000
      team_change: 50000
      initial_elo: 1200.0
  Runner:
    trainer_type: poca
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
    max_steps: 400000
    time_horizon: 256
    summary_freq: 10000
    checkpoint_interval: 100000
    keep_checkpoints: 5
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 25000
      swap_steps: 25000
      team_change: 50000
      initial_elo: 1200.0

environment_parameters:
  distance_shaping_coef: 0.0
```

- [ ] **Step 2: Write the shaped config**

Create `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\TagMApoca_shaped.yaml` — **identical to the sparse config above except the final block**:

```yaml
environment_parameters:
  distance_shaping_coef: 0.5
```

(Copy the sparse file verbatim and change only `distance_shaping_coef: 0.0` → `0.5`. Everything else — both behavior blocks, all hyperparameters, self-play — must match exactly so the only variable is the shaping coefficient.)

- [ ] **Step 3: Validate both configs load (user runs in the Anaconda Prompt)**

```cmd
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
python -c "import yaml; yaml.safe_load(open('config/poca/TagMApoca_sparse.yaml')); yaml.safe_load(open('config/poca/TagMApoca_shaped.yaml')); print('YAML OK')"
```
Expected: `YAML OK` (no parse error).

- [ ] **Step 4: Archive copies into this repo and commit**

Copy both files into `experiments/configs/` in this Unity project for thesis reproducibility, then:

```bash
git add experiments/configs/TagMApoca_sparse.yaml experiments/configs/TagMApoca_shaped.yaml
git commit -m "chore: archive sparse/shaped validation configs for reproducibility"
```

---

### Task 5: Run both validation arms and capture results

This task is an operational procedure, not code — no commit. The user runs the trainer in the Anaconda Prompt and presses Play in Unity (conda is unavailable in a normal shell — see CLAUDE.md).

- [ ] **Step 1: Run the SPARSE arm**

In the Anaconda Prompt (already in `(mlagents)`):

```cmd
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
mlagents-learn config/poca/TagMApoca_sparse.yaml --run-id=TagVal_sparse_01 --seed 12345
```
When it prints `Start training by pressing the Play button`, press Play in Unity.
Expected: both `Chaser` and `Runner` connect; summaries every 10k steps; runs to 400k; `.onnx` exported; clean exit; no NaNs. (~25-35 min at 8 arenas; arena count must be identical for both arms.)

- [ ] **Step 2: Run the SHAPED arm with the same seed**

```cmd
mlagents-learn config/poca/TagMApoca_shaped.yaml --run-id=TagVal_shaped_01 --seed 12345
```
Press Play when prompted. Same expectations.
Sanity check specific to this arm: the chaser's per-episode reward should reflect the extra shaping term (less uniformly negative than the sparse arm early on).

- [ ] **Step 3: Capture curves via TensorBoard (Playwright)**

In the second Anaconda Prompt:
```cmd
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
tensorboard --logdir results
```
Then (Claude drives Playwright) open `http://localhost:6006`, and screenshot, for both run-ids, the scalars: `Self-play/ELO`, `Environment/EpisodeLength`, `Environment/Catch`, `Environment/TimeToCatch`, `Environment/CumulativeReward`, `Environment/GroupCumulativeReward`. Save PNGs under `docs/figures/validation/`.

- [ ] **Step 4: Apply the decision rule and record findings**

Per the spec's strict bar, an arm is **healthy** only if, by 400k: **catch rate ↑** (above the ~15% random baseline) **AND mean episode length ↓** (below ~393 decision steps) **AND ELO diverges** (Chaser/Runner separate from 1200 in opposing directions).
- If at least the shaped arm is healthy → proceed toward the longer run; the sparse-vs-shaped delta is a thesis result.
- If **both** arms stay flat near baseline → trigger the **6/5 chaser-edge fallback** (set the chaser prefab `moveSpeed` to 6, re-run both arms), and record the stall as a finding.

Append the outcome (with the figure paths) to `docs/Theory.md` and `docs/progress.md`.

---

## Self-Review

**1. Spec coverage:**
- Two arms identical except shaping → Tasks 4 (configs differ only in coef) + 2 (coef drives it). ✓
- PBS form (Φ, F, γ, coef 0.5, maxDist) → Task 1 (math) + Task 2 (application). ✓
- Chaser-only, runner unchanged → Task 2 chaser branch only. ✓
- ±0.001/step kept in both arms → Task 2 leaves `AddReward(-0.001f)` in place. ✓
- Config-driven arm, not Editor toggle → Task 2 Step 3 reads `environment_parameters`; Task 4 sets it. ✓
- `catch` + `time_to_catch` stats → Task 3. ✓
- 400k budget, summary 10k → Task 4. ✓
- Same `--seed` → Task 5 Steps 1-2 (`--seed 12345`). ✓
- Strict 3-signal success rule + 6/5 fallback → Task 5 Step 4. ✓
- Kinematics 5/5 fixed (no code change now) → no task changes moveSpeed; fallback documented in Task 5 Step 4. ✓
- TensorBoard→Playwright capture → Task 5 Step 3. ✓

**2. Placeholder scan:** No TBD/TODO; every code step shows full code; configs are complete. ✓

**3. Type consistency:** `TagReward.PlanarDistance`, `TagReward.Potential(chaserPos, runnerPos, coef, maxDist)`, `TagReward.ShapingDelta(phiPrev, phiNext, gamma)` are defined in Task 1 and called with matching signatures in Task 2 (`CurrentPotential`, `OnActionReceived`). `distanceShapingCoef`, `prevPotential`, `arenaDiagonal`, `shapingGamma` declared in Task 2 Step 1 and used in Steps 2-4. `stats` declared in Task 3 Step 1, used Steps 2-4. Env-parameter key `distance_shaping_coef` matches between Task 2 Step 3 and Task 4. ✓
