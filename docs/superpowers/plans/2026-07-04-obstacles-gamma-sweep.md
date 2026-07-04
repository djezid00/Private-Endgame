# Obstacles × Gamma Sweep Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Run the obstacles × gamma experiment (spec `docs/superpowers/specs/2026-07-04-obstacles-gamma-sweep-design.md`): a 5-point gamma sweep (sparse reward) in a fixed- then random-obstacle arena, plus two low-γ shaped probes of the farming-trap mechanism.

**Architecture:** Approach B (pipelined). Sprint 1 ships the tiny `shaping_gamma` env-param + all configs + the probe batch, so the 2 probe runs train overnight while Sprint 2 builds the obstacle system (user-authored pillars, `ObstacleManager` activation/randomization, pure `ObstaclePlacement` math with EditMode tests). Every new binary passes a 50k smoke gate before 5M compute; Phase A → Phase B is a decision gate.

**Tech Stack:** Unity ML-Agents (MA-POCA, self-play), C# (Unity 6, NUnit EditMode tests), Python stdlib (config generator), Windows `.bat` batches run from the Anaconda Prompt (conda env `mlagents` — Claude cannot run `mlagents-learn`; the USER launches those steps).

**Conventions:**
- Unity project root: `c:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1`
- ML-Agents repo: `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents` (configs in `config\poca\`)
- Steps marked **[USER]** need the Unity Editor or the Anaconda Prompt — Claude prepares, the user executes and reports back.
- Commit after every task. No AI co-author trailers.

---

## Sprint 1 — shaped gamma probes (target: launch overnight tonight)

### Task 1: `shaping_gamma` env-param in TagAgent

**Files:**
- Modify: `Assets/Scripts/TagAgent.cs:52-68` (OnEpisodeBegin) and `:23-27` (fields)

The PBS term `F = γΦ′−Φ` must use the trainer's `extrinsic.gamma`. Today `shapingGamma` is a hard-coded inspector field (0.99). Make it config-driven with the inspector value as fallback, so **every existing config is byte-identical** (no param set ⇒ value unchanged).

- [ ] **Step 1: Edit `TagAgent.cs`**

Replace the private-state block (lines 25–27):

```csharp
    // Set once per episode from environment_parameters (0 in the sparse arm).
    private float distanceShapingCoef = 0f;
    private float prevPotential       = 0f;
```

with:

```csharp
    // Set once per episode from environment_parameters (0 in the sparse arm).
    private float distanceShapingCoef = 0f;
    private float prevPotential       = 0f;

    // One-time log so smoke runs can verify which shaping params the binary received.
    private static bool shapingParamsLogged = false;
```

Replace the chaser branch of `OnEpisodeBegin()` (lines 54–65):

```csharp
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
```

with:

```csharp
        if (teamId == 0)
        {
            arena.ResetArena();

            // Select this arm's shaping coefficient from the trainer config
            // (environment_parameters.distance_shaping_coef). 0 ⇒ sparse arm.
            distanceShapingCoef = Academy.Instance.EnvironmentParameters
                .GetWithDefault("distance_shaping_coef", 0f);

            // PBS gamma MUST track the trainer's extrinsic.gamma (gamma-sweep experiment).
            // Falls back to the inspector value ⇒ configs without the param are unchanged.
            shapingGamma = Academy.Instance.EnvironmentParameters
                .GetWithDefault("shaping_gamma", shapingGamma);

            if (!shapingParamsLogged)
            {
                Debug.Log($"[TagAgent] distance_shaping_coef={distanceShapingCoef:F2}, " +
                          $"shaping_gamma={shapingGamma:F3}");
                shapingParamsLogged = true;
            }

            // Seed Φ from the freshly-reset spawn so the first PBS delta is well-defined.
            prevPotential = CurrentPotential();
        }
```

Also update the field comment on line 23 from `// MUST match trainer extrinsic.gamma` to:

```csharp
    public float shapingGamma  = 0.99f;  // fallback only — overridden per episode by env-param "shaping_gamma"
```

- [ ] **Step 2 [USER]: Compile check + existing tests**

In the Unity Editor: let it recompile (no red Console errors), then Window > General > Test Runner > EditMode > Run All.
Expected: **5/5 `TagRewardTests` pass** (pure math untouched).

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/TagAgent.cs
git commit -m "feat: shaping_gamma env-param — PBS gamma follows trainer gamma (fallback = inspector value)"
```

### Task 2: Config generator — all 14 YAMLs from one template

**Files:**
- Create: `experiments/gen_gamma_configs.py`
- Generates 14 files into BOTH `C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca\` and `experiments/configs/`:
  - `TagMApoca_shaped_g080.yaml`, `TagMApoca_shaped_g090.yaml` (probes)
  - `TagMApoca_sparse_obsF_g{080,090,095,099,0995}.yaml` (Phase A)
  - `TagMApoca_sparse_obsR_g{080,090,095,099,0995}.yaml` (Phase B)
  - `TagMApoca_gprobe_smoke.yaml`, `TagMApoca_obs_smoke.yaml` (50k smoke gates)

A generator (not 14 hand-edited files) guarantees the only diffs vs the 5M rigor configs are gamma (both behavior blocks), `shaping_gamma`, and the obstacle env-params — and makes the sweep reproducible.

- [ ] **Step 1: Write `experiments/gen_gamma_configs.py`**

```python
"""Generate the obstacles x gamma sweep trainer configs (spec 2026-07-04).

Writes each YAML to the ml-agents config dir AND archives a copy in
experiments/configs/. Diffs vs TagMApoca_{sparse,shaped}_5M.yaml are ONLY:
gamma (both behavior blocks), shaping_gamma, and the obstacle env-params.
Run:  python experiments/gen_gamma_configs.py
"""
import os

MLAGENTS_CFG = r"C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents\config\poca"
ARCHIVE      = os.path.join(os.path.dirname(__file__), "configs")

GAMMAS = {"g080": 0.8, "g090": 0.9, "g095": 0.95, "g099": 0.99, "g0995": 0.995}


def behavior_block(name, gamma, max_steps, summary_freq, checkpoint_interval,
                   save_steps, swap_steps, team_change):
    return f"""  {name}:
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
        gamma: {gamma}
        strength: 1.0
    max_steps: {max_steps}
    time_horizon: 256
    summary_freq: {summary_freq}
    checkpoint_interval: {checkpoint_interval}
    keep_checkpoints: 20
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: {save_steps}
      swap_steps: {swap_steps}
      team_change: {team_change}
      initial_elo: 1200.0
"""


def config(header, gamma, env_params, smoke=False):
    if smoke:  # 50k budget, self-play cadences scaled down (same as TagMApoca_smoke.yaml)
        kw = dict(max_steps=50000, summary_freq=10000, checkpoint_interval=25000,
                  save_steps=5000, swap_steps=3000, team_change=20000)
    else:      # 5M rigor settings — identical to TagMApoca_{sparse,shaped}_5M.yaml
        kw = dict(max_steps=5000000, summary_freq=50000, checkpoint_interval=250000,
                  save_steps=50000, swap_steps=50000, team_change=100000)
    body = "behaviors:\n"
    body += behavior_block("Chaser", gamma, **kw)
    body += behavior_block("Runner", gamma, **kw)
    body += "\nenvironment_parameters:\n"
    for k, v in env_params.items():
        body += f"  {k}: {v}\n"
    return header + body


def env(coef, gamma, n_obs, layout):
    return {"distance_shaping_coef": coef, "shaping_gamma": gamma,
            "num_obstacles": n_obs, "obstacle_layout": layout}


FILES = {}

# Probes: shaped, no obstacles, low gamma (RQ-B)
for tag in ("g080", "g090"):
    g = GAMMAS[tag]
    FILES[f"TagMApoca_shaped_{tag}.yaml"] = config(
        f"# GAMMA PROBE (RQ-B) — MA-POCA shaped (PBS coef 0.5), NO obstacles, gamma {g}.\n"
        f"# Identical to TagMApoca_shaped_5M.yaml except gamma + shaping_gamma.\n"
        f"# Spec: docs/superpowers/specs/2026-07-04-obstacles-gamma-sweep-design.md\n",
        g, env(0.5, g, 0, 0))

# Phase A (fixed pillars) + Phase B (randomized): sparse sweep (RQ-A / RQ-C)
for mode, layout, word in (("obsF", 0, "FIXED"), ("obsR", 1, "RANDOM-PER-EPISODE")):
    for tag, g in GAMMAS.items():
        FILES[f"TagMApoca_sparse_{mode}_{tag}.yaml"] = config(
            f"# GAMMA SWEEP (RQ-A/RQ-C) — MA-POCA sparse, 4 {word} pillars, gamma {g}.\n"
            f"# Identical to TagMApoca_sparse_5M.yaml except gamma + obstacle env-params.\n"
            f"# Spec: docs/superpowers/specs/2026-07-04-obstacles-gamma-sweep-design.md\n",
            g, env(0.0, g, 4, layout))

# Smoke gates (50k)
FILES["TagMApoca_gprobe_smoke.yaml"] = config(
    "# SMOKE GATE for the gamma probes: 50k, shaped, gamma 0.8 — verifies the binary\n"
    "# reads shaping_gamma (check the [TagAgent] log line in the run log).\n",
    0.8, env(0.5, 0.8, 0, 0), smoke=True)
FILES["TagMApoca_obs_smoke.yaml"] = config(
    "# SMOKE GATE for the obstacle binary: 50k, sparse, 4 pillars in RANDOM mode\n"
    "# (exercises the placement code path harder than fixed mode).\n",
    0.99, env(0.0, 0.99, 4, 1), smoke=True)

for target in (MLAGENTS_CFG, ARCHIVE):
    os.makedirs(target, exist_ok=True)
    for name, text in FILES.items():
        with open(os.path.join(target, name), "w", newline="\n") as f:
            f.write(text)
    print(f"wrote {len(FILES)} configs -> {target}")
```

- [ ] **Step 2: Run the generator**

```bash
python experiments/gen_gamma_configs.py
```

Expected output: `wrote 14 configs -> ...config\poca` and `wrote 14 configs -> ...experiments/configs`.

- [ ] **Step 3: Verify one output against the rigor config**

```bash
diff <(grep -v '^#' experiments/configs/TagMApoca_shaped_5M.yaml) \
     <(grep -v '^#' experiments/configs/TagMApoca_shaped_g080.yaml)
```

Expected diff: ONLY the two `gamma: 0.99` → `gamma: 0.8` lines and the
`environment_parameters` block (adds `shaping_gamma: 0.8`, `num_obstacles: 0`,
`obstacle_layout: 0`). Any other diff = generator bug, fix before committing.

- [ ] **Step 4: Commit**

```bash
git add experiments/gen_gamma_configs.py experiments/configs/
git commit -m "feat: config generator + 14 gamma-sweep/probe/smoke trainer configs"
```

### Task 3: Probe batch script

**Files:**
- Create: `experiments/run_gamma_probes.bat`

- [ ] **Step 1: Write the batch (pattern of `run_overnight_poca.bat`)**

```bat
@echo off
setlocal
REM ============================================================================
REM  GAMMA PROBES (RQ-B) - 2 runs: shaped arm, NO obstacles, gamma 0.8 / 0.9.
REM  Tests the farming-trap mechanism (standing term scales with 1-gamma).
REM  Baseline for comparison: POCA_shaped_s{1,2,3} (gamma 0.99, already run).
REM
REM  PREREQUISITES
REM   1. Headless build REBUILT after the shaping_gamma commit (binary must
REM      contain the new env-param code) - check the [TagAgent] log line.
REM   2. Smoke gate TagMApoca_gprobe_smoke.yaml PASSED against this binary.
REM   3. Run from the Anaconda Prompt (conda env "mlagents").
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

echo Starting gamma probes at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_shaped_g080.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_g080_s1 --seed 1 > batch_logs\POCA_shaped_g080_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_shaped_g090.yaml --env="%ENV%" --no-graphics --run-id=POCA_shaped_g090_s1 --seed 1 > batch_logs\POCA_shaped_g090_s1.log 2>&1

echo Probes complete at %DATE% %TIME%
endlocal
pause
```

- [ ] **Step 2: Commit**

```bash
git add experiments/run_gamma_probes.bat
git commit -m "feat: run_gamma_probes.bat — overnight batch for the 2 shaped low-gamma probes"
```

### Task 4: Pre-registered expectations → Theory.md §14 (MUST land before any batch launches)

**Files:**
- Modify: `docs/Theory.md` (append after §13)
- Modify: `docs/progress.md` (new entry at top, below the header block)

- [ ] **Step 1: Append §14 to `docs/Theory.md`**

```markdown

---

## 14. Obstacles × gamma sweep — pre-registered expectations (written BEFORE the runs)

**New standing rule from this phase on:** every results section opens with a *pre-registered
expectations* subsection committed before the runs launch; findings are then reported against
these predictions. (Design: `docs/superpowers/specs/2026-07-04-obstacles-gamma-sweep-design.md`;
matrix: 2 shaped low-γ probes + {fixed, random} 4-pillar arenas × γ ∈ {0.8, 0.9, 0.95, 0.99,
0.995}, sparse, 3 seeds at the endpoints, 5M steps/behavior.)

### Expectations — RQ-B (shaped probes, no obstacles, γ = 0.8 / 0.9)

For a stationary agent the per-step PBS reward is `F = γΦ − Φ = (1−γ)·coef·(d/maxDist) ≥ 0` —
the invariance-violating "standing" term scales with **(1−γ)**. At γ=0.8 it is **20×** the
γ=0.99 case, and the future terminal +1 is simultaneously discounted harder. Both effects point
the same way.

> **Prediction:** farming *worsens* at lower γ — catch rate ≤ the ~0.01 γ=0.99 baseline, Group
> Reward pinned ≈ −1. A material catch-rate **rise** at low γ falsifies the mechanism story.
> Secondary signature to watch: the standing term grows with *distance*, so the γ=0.8 chaser may
> drift to keep distance rather than hover close (this derivation refines §11's "standing reward
> for being close" phrasing — reconcile explicitly when writing up).

### Expectations — RQ-A (sparse gamma sweep, obstacle arenas)

γ sets the effective planning horizon (~1/(1−γ) decisions): γ=0.8 ≈ 5 decisions (~25 physics
steps), γ=0.99 ≈ 100, γ=0.995 ≈ 200 (half the episode cap).

> **Prediction:** catch rate and ELO gap **rise with γ up to ~0.99, then plateau or dip slightly
> at 0.995** (saturating / inverted-U curve): γ=0.8 is too myopic to plan interception around
> cover; 0.995 adds credit-assignment noise with little extra planning benefit. Falsified if the
> curve is flat (γ irrelevant here) or monotonic in the opposite direction.

### Expectations — RQ-C (obstacles)

> **Prediction:** with 4 fixed pillars the sparse γ=0.99 chaser still clearly beats the runner
> but below the open-arena ceiling (catch rate ≫ the ~0.1 random baseline, < the ~1.0 open-arena
> result); randomized layouts learn slower and end lower than fixed at matched γ. Qualitative:
> runner uses pillars to break line of sight; chaser learns cut-off routes (fixed) vs general
> navigation (random).

*Results land here after each batch: probe figure (catch rate for shaped γ ∈ {0.8, 0.9, 0.99}),
sensitivity curves (catch rate & ELO gap vs γ, per obstacle phase, error bars at the 3-seed
endpoints), fixed-vs-random contrast, qualitative behavior notes.*
```

- [ ] **Step 2: Add a `docs/progress.md` entry (at the top, below the file header)**

```markdown
## 2026-07-04 — Obstacles × gamma sweep: spec + plan + probes prepared

New branch `feat/obstacles-gamma-sweep` (off `feat/ppo-comparison`). Brainstormed + spec'd the
next phase (spec `2026-07-04-obstacles-gamma-sweep-design.md`): **5-point gamma sweep**
(0.8–0.995, sparse, 4-pillar arenas fixed→random, 3 seeds at endpoints) + **2 shaped low-γ
probes** of the farming trap. Key pre-registered prediction (Theory §14): the PBS standing term
scales with (1−γ), so **lower γ should farm WORSE** — either outcome is citable.

Sprint 1 done: `shaping_gamma` env-param in `TagAgent` (fallback = inspector ⇒ old configs
byte-identical), config generator (14 YAMLs), `run_gamma_probes.bat`, Theory §14 expectations
pre-registered. **Next:** USER rebuilds headless binary → gprobe smoke gate → launch probes
overnight; Sprint 2 (obstacle system) builds while they run.

---
```

- [ ] **Step 3: Commit**

```bash
git add docs/Theory.md docs/progress.md
git commit -m "docs: pre-register gamma-sweep expectations (Theory §14) before any runs"
```

### Task 5 [USER]: Rebuild headless binary + probe smoke gate

- [ ] **Step 1 [USER]: Editor verify.** Unity recompiles clean; EditMode tests 5/5 (already done in Task 1); prefab agents Behavior Type = **Default**, Model = empty; scene = the 16-arena build scene.

- [ ] **Step 2 [USER]: Rebuild the headless player** (File > Build Settings > Windows Standalone > Build) to `Build\TagMApoca_V1.exe`. The binary MUST postdate the Task 1 commit.

- [ ] **Step 3 [USER]: Run the smoke gate** (Anaconda Prompt, ml-agents repo root):

```
mlagents-learn config\poca\TagMApoca_gprobe_smoke.yaml --env="C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe" --no-graphics --run-id=GprobeSmoke_01 --seed 1
```

- [ ] **Step 4: Verify gate criteria (Claude checks the run log + results with the user):**
  1. Player log / console contains `[TagAgent] distance_shaping_coef=0.50, shaping_gamma=0.800` — the binary reads the new param. (Player log: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\TagMApoca_V1\Player.log`.)
  2. Run completes 50k, both behaviors, finite `Baseline Loss` (still POCA), no NaN, `.onnx` exported.
  3. Chaser cumulative reward clearly differs from the γ=0.99 shaped smoke history (the ×20 standing term is visible even at 50k).

**Gate:** all 3 pass → Task 6. Log line missing → the binary is stale (rebuild) or the param name mismatches (fix, recommit, rebuild).

### Task 6 [USER]: Launch the probes (overnight #1)

- [ ] **Step 1 [USER]:** From the Anaconda Prompt: `experiments\run_gamma_probes.bat` (~8 h for both).
- [ ] **Step 2:** Next session: confirm both `results/POCA_shaped_g{080,090}_s1/` have final `Chaser.onnx`/`Runner.onnx`; record final Catch rate / ELO / Group Reward vs the §14 predictions.

---

## Sprint 2 — obstacle system (build while the probes run)

### Task 7: `ObstaclePlacement` pure math (TDD)

**Files:**
- Create: `Assets/Scripts/Reward/ObstaclePlacement.cs` (TagGame.Reward asmdef — the unit-tested assembly; despite the folder name it is the project's pure-logic assembly)
- Create: `Assets/Tests/EditMode/ObstaclePlacementTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
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
```

- [ ] **Step 2 [USER]: Run tests — expect compile FAIL** (`ObstaclePlacement` does not exist). Test Runner > EditMode. This confirms the tests exercise the new type.

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/Reward/ObstaclePlacement.cs`:

```csharp
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
```

- [ ] **Step 4 [USER]: Run tests — expect 12/12 PASS** (5 TagReward + 7 ObstaclePlacement).

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Reward/ObstaclePlacement.cs Assets/Tests/EditMode/ObstaclePlacementTests.cs
git add Assets/Scripts/Reward/ObstaclePlacement.cs.meta Assets/Tests/EditMode/ObstaclePlacementTests.cs.meta
git commit -m "feat: ObstaclePlacement pure placement math + 7 EditMode tests (TDD)"
```

(`.meta` files appear after the Editor imports the new scripts — stage them with the code.)

### Task 8: `ObstacleManager` component

**Files:**
- Create: `Assets/Scripts/ObstacleManager.cs` (main assembly, next to `TagArenaManager.cs`)

- [ ] **Step 1: Write the component**

```csharp
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Drives the user-authored obstacle pillars per episode from two trainer env-params:
///   num_obstacles   (default 0)  — how many pillars are active; 0 = legacy open arena.
///   obstacle_layout (default 0)  — 0 = user's authored (fixed) layout, 1 = randomize
///                                  position + Y-rotation each episode.
/// Attach to the "Obstacles" parent inside the TagArena prefab; pillars are its children
/// (tag "Wall", static BoxColliders, ~2u tall). Placement math is ObstaclePlacement
/// (unit-tested); on a placement failure the authored layout is kept — never break training.
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("Authored pillars (auto-collected from children if left empty)")]
    public Transform[] pillars;

    [Header("Placement rules (random mode)")]
    public float arenaHalfSize  = 10f;  // 20x20 floor
    public float wallClearance  = 1.5f;
    public float minSeparation  = 4f;
    public float agentClearance = 1.5f; // used by TagArenaManager spawn rejection

    private readonly System.Random rng = new System.Random();
    private readonly List<Vector2> positions = new List<Vector2>(); // active obstacle XZ
    private Vector3[]    authoredLocalPos;
    private Quaternion[] authoredLocalRot;

    private static bool paramsLogged = false;

    private void Awake()
    {
        if (pillars == null || pillars.Length == 0)
        {
            pillars = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                pillars[i] = transform.GetChild(i);
        }
        authoredLocalPos = new Vector3[pillars.Length];
        authoredLocalRot = new Quaternion[pillars.Length];
        for (int i = 0; i < pillars.Length; i++)
        {
            authoredLocalPos[i] = pillars[i].localPosition;
            authoredLocalRot[i] = pillars[i].localRotation;
        }
    }

    /// <summary>Called by TagArenaManager.ResetArena() BEFORE agents spawn.</summary>
    public void ResetObstacles()
    {
        int active = Mathf.Clamp(Mathf.RoundToInt(Academy.Instance.EnvironmentParameters
                         .GetWithDefault("num_obstacles", 0f)), 0, pillars.Length);
        bool random = Academy.Instance.EnvironmentParameters
                         .GetWithDefault("obstacle_layout", 0f) > 0.5f;

        if (!paramsLogged)
        {
            Debug.Log($"[ObstacleManager] num_obstacles={active}, layout={(random ? "random" : "fixed")}");
            paramsLogged = true;
        }

        for (int i = 0; i < pillars.Length; i++)
            pillars[i].gameObject.SetActive(i < active);

        positions.Clear();
        if (active == 0) return;

        if (random && ObstaclePlacement.TryPlaceObstacles(active, arenaHalfSize, wallClearance,
                                                          minSeparation, rng, positions))
        {
            for (int i = 0; i < active; i++)
            {
                pillars[i].localPosition = new Vector3(positions[i].x,
                                                       authoredLocalPos[i].y,
                                                       positions[i].y);
                pillars[i].localRotation = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);
            }
        }
        else
        {
            // Fixed mode — or random-placement failure fallback: the authored layout.
            for (int i = 0; i < active; i++)
            {
                pillars[i].localPosition = authoredLocalPos[i];
                pillars[i].localRotation = authoredLocalRot[i];
                positions.Add(new Vector2(authoredLocalPos[i].x, authoredLocalPos[i].z));
            }
        }
    }

    /// <summary>Spawn-safety query for TagArenaManager (arena-local position).</summary>
    public bool IsClearOfActiveObstacles(Vector3 localPos)
    {
        return ObstaclePlacement.IsClearOfObstacles(new Vector2(localPos.x, localPos.z),
                                                    positions, positions.Count, agentClearance);
    }
}
```

- [ ] **Step 2 [USER]: Editor compile check** — no Console errors; EditMode tests still 12/12.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/ObstacleManager.cs Assets/Scripts/ObstacleManager.cs.meta
git commit -m "feat: ObstacleManager — env-param-driven pillar activation + per-episode randomization"
```

### Task 9: TagArenaManager integration (obstacles reset first, spawns avoid pillars)

**Files:**
- Modify: `Assets/Scripts/TagArenaManager.cs:9-25` (fields) and `:78-127` (ResetArena)

- [ ] **Step 1: Add the reference field**

After the `[Header("Agent References")]` block (below line 11, `public TagAgent runner;`), add:

```csharp
    [Header("Obstacles (optional — leave empty for the legacy open arena)")]
    public ObstacleManager obstacles;   // drag the TagArena prefab's Obstacles object here
```

- [ ] **Step 2: Replace `ResetArena()` (lines 78–127) with the obstacle-aware version**

```csharp
    public void ResetArena()
    {
        // Reset episode state flags
        episodeEnded = false;
        stepCount    = 0;

        // Obstacles FIRST, agents second — agent spawn rejection needs the new positions.
        if (obstacles != null) obstacles.ResetObstacles();

        // --- Place chaser on the LEFT half, runner on the RIGHT half ---
        // Separated sides prevent instant-collision on spawn; SampleSpawn keeps
        // both out of the obstacle clearance zone.
        Vector3 chaserPos = SampleSpawn(-arenaRadius + 1f, -1f);
        Vector3 runnerPos = SampleSpawn(1f, arenaRadius - 1f);

        // --- Safety loop: retry runner position if too close to chaser ---
        int attempts = 0;
        while (Vector3.Distance(chaserPos, runnerPos) < minSpawnDistance
               && attempts < spawnRetryLimit)
        {
            runnerPos = SampleSpawn(1f, arenaRadius - 1f);
            attempts++;
        }

        // --- Apply positions and random rotations ---
        chaser.transform.localPosition = chaserPos;
        runner.transform.localPosition = runnerPos;

        chaser.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        runner.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // --- Zero out all physics velocity ---
        chaserRb.linearVelocity  = Vector3.zero;
        chaserRb.angularVelocity = Vector3.zero;
        runnerRb.linearVelocity  = Vector3.zero;
        runnerRb.angularVelocity = Vector3.zero;
    }

    // Samples a spawn in [xMin,xMax] x [-arenaRadius+1, arenaRadius-1], re-rolling while the
    // candidate sits inside an active obstacle's clearance. Bounded retries; after the budget
    // it returns the last candidate rather than loop forever (an occasional pillar-adjacent
    // spawn is harmless — physics pushes the box out).
    private Vector3 SampleSpawn(float xMin, float xMax)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < spawnRetryLimit; i++)
        {
            pos = new Vector3(
                Random.Range(xMin, xMax),
                spawnY,
                Random.Range(-arenaRadius + 1f, arenaRadius - 1f));
            if (obstacles == null || obstacles.IsClearOfActiveObstacles(pos)) return pos;
        }
        return pos;
    }
```

- [ ] **Step 3 [USER]: Editor compile check** — no Console errors; EditMode tests still 12/12; press Play briefly WITHOUT a trainer (Heuristic): arena resets normally with `obstacles` left empty (legacy path intact).

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/TagArenaManager.cs
git commit -m "feat: arena reset drives obstacles first; agent spawns reject obstacle-clearance zones"
```

### Task 10 [USER]: Author the pillars in the TagArena prefab (Claude reviews)

- [ ] **Step 1 [USER]: In the Unity Editor, edit `Assets/Prefabs/TagArena.prefab` (open in Prefab Mode):**
  1. Add an empty child `Obstacles` at local position (0, 0, 0); add the **ObstacleManager** component to it (leave `pillars` empty — it auto-collects children; defaults are fine).
  2. Under it create **4 cubes** (right-click > 3D Object > Cube), e.g. `Pillar0..Pillar3`:
     - **Scale (2, 2, 2)**, local **Y position = 1** (rests on the floor), your choice of XZ — mirror-symmetric across BOTH axes so neither role is favored (suggestion: (±4, 1, ±4)), ≥1.5u from the walls and outside the spawn bubbles.
     - **Tag = `Wall`** (raycasts then see them with NO observation-space change).
     - Keep the default BoxCollider (NOT trigger); **no Rigidbody** (static).
     - **Deactivate all 4** (uncheck the checkbox next to each pillar's name) — `num_obstacles: 0` configs must see the legacy arena even before `ResetObstacles()` runs.
  3. On the arena's **TagArenaManager** component, drag the `Obstacles` object into the new `obstacles` field.
  4. Save the prefab; confirm all arena copies in the scene show the pillars (inactive).

- [ ] **Step 2: Claude reviews the prefab YAML** — read `Assets/Prefabs/TagArena.prefab` and verify: 4 pillars under `Obstacles`, tag `Wall`, scale (2,2,2), y=1, symmetric XZ, colliders present, no Rigidbody, all inactive, ObstacleManager present, TagArenaManager.obstacles wired. Report any deviation before committing.

- [ ] **Step 3 [USER]: Heuristic play-test (WASD)** with the pillars temporarily active (set them active in the scene instance, or run once with a local config change): the agent collides with pillars (cannot pass through), rays are blocked (Ray Perception debug gizmo shows hits), episode reset works. Undo any temporary scene tweak afterwards.

- [ ] **Step 4: Commit**

```bash
git add Assets/Prefabs/TagArena.prefab Assets/Scenes/
git commit -m "feat: 4 hand-authored obstacle pillars (Wall tag, inactive) + ObstacleManager wired into TagArena"
```

### Task 11: Phase batch scripts

**Files:**
- Create: `experiments/run_obs_phaseA.bat`
- Create: `experiments/run_obs_phaseB.bat`

- [ ] **Step 1: Write `experiments/run_obs_phaseA.bat`**

```bat
@echo off
setlocal
REM ============================================================================
REM  GAMMA SWEEP PHASE A (RQ-A/RQ-C) - sparse, 4 FIXED pillars, 9 runs (~36-42h):
REM  gamma 0.8 x3 seeds, 0.9 / 0.95 / 0.99 x1, 0.995 x3 seeds.
REM  PREREQ: obstacle binary rebuilt + TagMApoca_obs_smoke.yaml gate PASSED.
REM  Run from the Anaconda Prompt (conda env "mlagents").
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

echo Starting Phase A at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_sparse_obsF_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g080_s1  --seed 1 > batch_logs\POCA_sparse_obsF_g080_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g080_s2  --seed 2 > batch_logs\POCA_sparse_obsF_g080_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g080_s3  --seed 3 > batch_logs\POCA_sparse_obsF_g080_s3.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g090.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g090_s1  --seed 1 > batch_logs\POCA_sparse_obsF_g090_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g095.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g095_s1  --seed 1 > batch_logs\POCA_sparse_obsF_g095_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g099.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g099_s1  --seed 1 > batch_logs\POCA_sparse_obsF_g099_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g0995_s1 --seed 1 > batch_logs\POCA_sparse_obsF_g0995_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g0995_s2 --seed 2 > batch_logs\POCA_sparse_obsF_g0995_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsF_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsF_g0995_s3 --seed 3 > batch_logs\POCA_sparse_obsF_g0995_s3.log 2>&1

echo Phase A complete at %DATE% %TIME%
endlocal
pause
```

- [ ] **Step 2: Write `experiments/run_obs_phaseB.bat`** — identical structure with `obsF` → `obsR` in every config name, run-id, and log name, and the header line `FIXED pillars` → `RANDOM-PER-EPISODE pillars`, plus this extra header line: `REM  GATE: launch only after the Phase A review (decision gate in the plan).`

```bat
@echo off
setlocal
REM ============================================================================
REM  GAMMA SWEEP PHASE B (RQ-A/RQ-C) - sparse, 4 RANDOM-PER-EPISODE pillars, 9 runs:
REM  gamma 0.8 x3 seeds, 0.9 / 0.95 / 0.99 x1, 0.995 x3 seeds.
REM  GATE: launch only after the Phase A review (decision gate in the plan).
REM  PREREQ: same binary as Phase A (no code change between phases).
REM  Run from the Anaconda Prompt (conda env "mlagents").
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

echo Starting Phase B at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_sparse_obsR_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g080_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g080_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g080_s2  --seed 2 > batch_logs\POCA_sparse_obsR_g080_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g080.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g080_s3  --seed 3 > batch_logs\POCA_sparse_obsR_g080_s3.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g090.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g090_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g090_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g095.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g095_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g095_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g099.yaml  --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g099_s1  --seed 1 > batch_logs\POCA_sparse_obsR_g099_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g0995_s1 --seed 1 > batch_logs\POCA_sparse_obsR_g0995_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g0995_s2 --seed 2 > batch_logs\POCA_sparse_obsR_g0995_s2.log 2>&1
mlagents-learn %CFG%\TagMApoca_sparse_obsR_g0995.yaml --env="%ENV%" --no-graphics --run-id=POCA_sparse_obsR_g0995_s3 --seed 3 > batch_logs\POCA_sparse_obsR_g0995_s3.log 2>&1

echo Phase B complete at %DATE% %TIME%
endlocal
pause
```

- [ ] **Step 3: Commit**

```bash
git add experiments/run_obs_phaseA.bat experiments/run_obs_phaseB.bat
git commit -m "feat: Phase A/B batch scripts — 9-run gamma sweeps (fixed / random pillars)"
```

### Task 12 [USER]: Rebuild binary #2 + obstacle smoke gate

- [ ] **Step 1 [USER]:** Confirm scene/prefab state for training (agents Behavior Type **Default**, Model empty), then **rebuild** `Build\TagMApoca_V1.exe`. Binary must postdate the Task 10 prefab commit.

- [ ] **Step 2 [USER]: Run the obstacle smoke** (random mode — the harder code path):

```
mlagents-learn config\poca\TagMApoca_obs_smoke.yaml --env="C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe" --no-graphics --run-id=ObsSmoke_01 --seed 1
```

- [ ] **Step 3: Verify gate criteria (Claude checks with the user):**
  1. Player log contains `[ObstacleManager] num_obstacles=4, layout=random` (binary reads the params).
  2. Run completes 50k, both behaviors, finite Baseline Loss, no NaN, no Unity error spam in the Player log.
  3. `Environment/Catch` > 0 in TensorBoard (catches still physically possible among pillars) and `Environment/TimeToCatch` nonzero.
  4. No cross-arena anomalies (episode lengths and rewards in normal ranges — a pillar can never leave its arena since placement is arena-local with wall clearance).

**Gate:** all pass → Phase A. Criterion 3 fails (no catches at all in 50k) → likely over-crowded arena: reduce pillar scale to (2,2,2)→(1.5,2,1.5) or `minSeparation` 4→5, re-verify, re-build, re-smoke — document the change.

### Task 13 [USER]: Phase A launch → review (decision gate) → Phase B launch

- [ ] **Step 1 [USER]:** `experiments\run_obs_phaseA.bat` (~36–42 h unattended).
- [ ] **Step 2 (with Claude): Phase A review against Theory §14 expectations:**
  - All 9 `results/POCA_sparse_obsF_*` complete with final `.onnx`; pull final `Environment/Catch`, ELO, Group Reward, Episode Length per run (batch logs + TensorBoard).
  - Plot/tabulate catch rate vs γ (endpoint cells as mean±range of 3 seeds). Compare against the §14 RQ-A prediction (rise to ~0.99, plateau/dip at 0.995).
  - **Pre-registered fallback check:** if the γ=0.99 cell is still at the random baseline (catch ≈ 0.1, episodes ≈ cap), Phase A reruns with 2 pillars (`num_obstacles: 2` variants) instead of proceeding.
  - Write the Phase A results into Theory §14 (against the predictions) + progress.md entry; commit.
- [ ] **Step 3 (decision gate):** USER decides: proceed to Phase B as-is / adjust / stop. Default = proceed if the sweep shows a resolvable trend.
- [ ] **Step 4 [USER]:** `experiments\run_obs_phaseB.bat` (~36–42 h).

### Task 14: Final analysis + write-up

- [ ] **Step 1 (with Claude):** Aggregate all cells: probes (vs `POCA_shaped_s*` baseline), Phase A, Phase B (vs `POCA_sparse_s*` open-arena anchor). Produce the §14 figures into `docs/figures/gamma/`:
  - `probe_gamma_trap.png` — shaped catch rate, γ ∈ {0.8, 0.9, 0.99-baseline} (extends §13 Fig 7 style: Okabe-Ito colours, end-labelled).
  - `sweep_catch_vs_gamma.png` + `sweep_elo_vs_gamma.png` — 5-point curves, one line per obstacle phase, error bars at the 3-seed endpoints, open-arena γ=0.99 anchor marked.
  - `fixed_vs_random.png` — matched-γ contrast.
- [ ] **Step 2:** Complete Theory §14: results vs each pre-registered prediction (explicitly: confirmed / falsified / partial), the §11 "standing reward" phrasing reconciliation, caveats (1 seed at interior γ points, ELO relative, obstacle runs single environment geometry).
- [ ] **Step 3:** Update `docs/progress.md` + `CLAUDE.md` recap block; commit; then invoke `superpowers:finishing-a-development-branch` (NO merge to main without explicit user approval).
- [ ] **Step 4 [USER] (optional, thesis-visual):** Editor inference with the best fixed-obstacle brains — watch for cover use / cut-off routes; note qualitative observations into §14.

---

## Self-review (done at write time)

- **Spec coverage:** shaping_gamma param (T1), configs incl. smoke (T2), probe batch (T3), pre-registered expectations before launch (T4), rebuild+smoke gates (T5, T12), probes (T6), placement math + tests (T7), ObstacleManager (T8), spawn integration (T9), user-authored pillars + review (T10), phase batches (T11), decision gate (T13), analysis/figures/§14 (T14). Fallbacks: placement-failure fallback (T7/T8), no-catch smoke fallback (T12), 2-pillar Phase A fallback (T13).
- **Type consistency:** `ObstaclePlacement.TryPlaceObstacles(int, float, float, float, System.Random, List<Vector2>)` and `IsClearOfObstacles(Vector2, IReadOnlyList<Vector2>, int, float)` match their call sites in `ObstacleManager`; `ObstacleManager.ResetObstacles()` / `IsClearOfActiveObstacles(Vector3)` match `TagArenaManager` call sites; run-ids in `.bat` files match config filenames from the generator.
- **Placeholder scan:** every code step shows full code; every command shows expected output; no TBDs.
