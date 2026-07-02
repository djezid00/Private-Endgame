# PPO Comparison Arm Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a config-driven `individual_terminal_reward` flag and two PPO trainer configs so we can run a 2×2 (algorithm × reward) comparison — `PPO_sparse_s1` + `PPO_shaped_s1` against the existing 3-seed MA-POCA bands.

**Architecture:** `TagArenaManager` already delivers the terminal ±1 (plus time/survival bonuses) via `SimpleMultiAgentGroup.AddGroupReward`. PPO ignores group rewards, so we add a guarded branch that ALSO delivers the identical values via each agent's `Agent.AddReward`, activated only when the env-param `individual_terminal_reward > 0`. The flag defaults off, leaving the POCA path byte-identical. Everything else (obs, network, self-play) is shared, so the only variables are trainer_type and `distance_shaping_coef`.

**Tech Stack:** Unity ML-Agents (C# `Agent`/`SimpleMultiAgentGroup`/`Academy.EnvironmentParameters`), ml-agents `mlagents-learn` (poca vs ppo trainer), Windows `.bat` batch runner, headless standalone build.

**Design reference:** `docs/superpowers/specs/2026-07-02-ppo-comparison-design.md`

**Environment note:** conda only activates inside the Anaconda Prompt, so **Claude cannot run `mlagents-learn` or Unity itself** — steps that train, smoke-test, build, or run EditMode tests are executed by the **user**, who pastes the prepared command / clicks the Editor button and reports the result. Claude writes code and configs.

---

## File Structure

| File | Responsibility | Action |
|---|---|---|
| `Assets/Scripts/TagArenaManager.cs` | Adds the `individual_terminal_reward` guarded mirror in `OnAgentTagged` + `TriggerStalemate`, plus a one-line helper. Only file with C# changes. | Modify |
| `C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/config/ppo/TagMApoca_ppo_sparse.yaml` | PPO trainer config, sparse arm (`distance_shaping_coef: 0.0`, `individual_terminal_reward: 1.0`). | Create |
| `…/ml-agents/config/ppo/TagMApoca_ppo_shaped.yaml` | PPO trainer config, shaped arm (`distance_shaping_coef: 0.5`). | Create |
| `…/ml-agents/config/ppo/TagMApoca_ppo_smoke.yaml` | 50k-step PPO smoke config (shaped) for the pre-run gate. | Create |
| `experiments/configs/TagMApoca_ppo_sparse.yaml` / `_shaped.yaml` / `_smoke.yaml` | Archived copies (reproducibility), mirroring the existing `experiments/configs/` convention. | Create |
| `experiments/run_ppo.bat` | Unattended runner for the two 5M PPO runs, per-run logs. | Create |

---

## Task 1: `individual_terminal_reward` guarded mirror in TagArenaManager

**Files:**
- Modify: `Assets/Scripts/TagArenaManager.cs` (`OnAgentTagged` ~175-217, `TriggerStalemate` ~148-167, add helper)

The mirror reuses the exact reward values the groups already receive. `chaser` and `runner` are the manager's own `TagAgent` fields (the real chaser/runner), so a catch is always scored chaser-wins regardless of which collider fired. `Agent.AddReward(float)` is public.

- [ ] **Step 1: Add the helper method**

Add this private method to `TagArenaManager` (e.g. just above `TriggerStalemate`):

```csharp
    // ─────────────────────────────────────────────
    // PPO SUPPORT — individual terminal reward toggle
    // PPO ignores group rewards (AddGroupReward / EndGroupEpisode are POCA-only), so a PPO run would
    // train with no win/lose signal. When the config sets individual_terminal_reward > 0 we ALSO
    // deliver the terminal ±1 (plus the same time/survival bonuses) through each agent's individual
    // AddReward. At group-size-1 this is exactly equivalent to the group reward, so POCA and PPO see
    // the same signal and the comparison stays fair. Defaults to 0 ⇒ the POCA path is byte-identical.
    // ─────────────────────────────────────────────
    private bool IndividualTerminalRewardOn()
        => Academy.Instance.EnvironmentParameters.GetWithDefault("individual_terminal_reward", 0f) > 0.5f;
```

- [ ] **Step 2: Mirror the stalemate reward in `TriggerStalemate`**

In `TriggerStalemate`, immediately after the two `AddGroupReward` lines, add the guarded mirror. The block becomes:

```csharp
        // Runner group survived the full episode — reward it
        runnerGroup.AddGroupReward(+1f);
        // Chaser group failed to catch the runner — penalise it
        chaserGroup.AddGroupReward(-1f);

        // PPO also needs the win/lose signal individually (see IndividualTerminalRewardOn).
        if (IndividualTerminalRewardOn())
        {
            runner.AddReward(+1f);
            chaser.AddReward(-1f);
        }
```

- [ ] **Step 3: Mirror the catch reward in `OnAgentTagged`**

In `OnAgentTagged`, replace the `if (tagger.teamId == 0) { … } else { … }` reward block with the version below. `timeBonus`/`survivalBonus` are computed once and reused for both the group and the individual channel (DRY — guaranteed-identical values):

```csharp
        bool mirror = IndividualTerminalRewardOn();

        if (tagger.teamId == 0) // ── CHASER caught RUNNER ─────────────────────
        {
            // Chaser group reward: base +1 plus a time bonus up to +0.5
            // (catches faster = bigger bonus → chaser learns urgency)
            float timeBonus = (1f - taggerProgress) * 0.5f;
            // Runner group reward: base -1 but survival softens penalty up to +0.5
            // (survived longer = smaller net penalty → runner learns to dodge)
            float survivalBonus = taggedProgress * 0.5f;

            chaserGroup.AddGroupReward(1f + timeBonus);
            runnerGroup.AddGroupReward(-1f + survivalBonus);

            if (mirror)
            {
                chaser.AddReward(1f + timeBonus);
                runner.AddReward(-1f + survivalBonus);
            }
        }
        else // ── RUNNER somehow triggered the collision (edge case) ──────────
        {
            // A catch is a catch: chaser side wins regardless of which collider fired.
            chaserGroup.AddGroupReward( 1f);
            runnerGroup.AddGroupReward(-1f);

            if (mirror)
            {
                chaser.AddReward( 1f);
                runner.AddReward(-1f);
            }
        }
```

- [ ] **Step 4: (USER) Verify Unity compiles clean**

Alt-tab to the Unity Editor, let it recompile. Expected: **no Console errors/warnings** from `TagArenaManager.cs`.

- [ ] **Step 5: (USER) Run EditMode tests**

Window > General > Test Runner > EditMode > **Run All**. Expected: **5/5 pass** (the `TagRewardTests` shaping math is untouched; this confirms no incidental breakage).

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/TagArenaManager.cs
git commit -m "feat: individual_terminal_reward toggle mirrors terminal reward via AddReward for PPO"
```

---

## Task 2: PPO trainer configs (sparse, shaped, smoke)

**Files:**
- Create: `…/ML_AGENTS_GIT/ml-agents/config/ppo/TagMApoca_ppo_sparse.yaml`
- Create: `…/ml-agents/config/ppo/TagMApoca_ppo_shaped.yaml`
- Create: `…/ml-agents/config/ppo/TagMApoca_ppo_smoke.yaml`
- Create: `experiments/configs/TagMApoca_ppo_sparse.yaml`, `_shaped.yaml`, `_smoke.yaml` (identical archived copies)

Same `network_settings`, `self_play`, and shared hyperparameters as `config/poca/TagMApoca_sparse_5M.yaml`; only `trainer_type` (poca→ppo), `environment_parameters`, and (for smoke) `max_steps` differ. PPO uses the same `batch_size`/`buffer_size`/`beta`/`epsilon`/`lambd`/`num_epoch` fields.

- [ ] **Step 1: Create `config/ppo/TagMApoca_ppo_sparse.yaml`**

```yaml
# 1v1 RIGOR PHASE — PPO, SPARSE arm (no distance shaping), 5M budget.
# 2x2 comparison vs MA-POCA. Identical network/self_play to the POCA 5M configs; only
# trainer_type (poca->ppo) and environment_parameters differ. individual_terminal_reward=1.0
# makes TagArenaManager ALSO deliver the terminal ±1 via AddReward, because PPO ignores group rewards.
# Run seed 1 so it lines up with POCA_sparse_s1.
behaviors:
  Chaser:
    trainer_type: ppo
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
    max_steps: 5000000
    time_horizon: 256
    summary_freq: 50000
    checkpoint_interval: 250000
    keep_checkpoints: 20
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 50000
      swap_steps: 50000
      team_change: 100000
      initial_elo: 1200.0
  Runner:
    trainer_type: ppo
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
    max_steps: 5000000
    time_horizon: 256
    summary_freq: 50000
    checkpoint_interval: 250000
    keep_checkpoints: 20
    self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 50000
      swap_steps: 50000
      team_change: 100000
      initial_elo: 1200.0

environment_parameters:
  distance_shaping_coef: 0.0
  individual_terminal_reward: 1.0
```

- [ ] **Step 2: Create `config/ppo/TagMApoca_ppo_shaped.yaml`**

Identical to Step 1 **except** the header word "SPARSE"→"SHAPED" and the final block:

```yaml
environment_parameters:
  distance_shaping_coef: 0.5
  individual_terminal_reward: 1.0
```

- [ ] **Step 3: Create `config/ppo/TagMApoca_ppo_smoke.yaml`**

Copy of the shaped config with a short budget for the pre-run gate. Change **every** `max_steps: 5000000` to `max_steps: 50000` (both behavior blocks), change `checkpoint_interval: 250000` to `checkpoint_interval: 25000`, `summary_freq: 50000` to `summary_freq: 10000`, keep `distance_shaping_coef: 0.5` and `individual_terminal_reward: 1.0`. Update the header comment to "PPO SMOKE (50k) — pre-run gate: does PPO train on the individual reward and tolerate grouped agents?".

- [ ] **Step 4: Copy all three into the archive**

```bash
cd "c:/Users/david/Documents/PROGRAMMING/UnityProjects/TagMApoca_V1"
SRC="C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/config/ppo"
cp "$SRC/TagMApoca_ppo_sparse.yaml" experiments/configs/
cp "$SRC/TagMApoca_ppo_shaped.yaml" experiments/configs/
cp "$SRC/TagMApoca_ppo_smoke.yaml"  experiments/configs/
```

- [ ] **Step 5: Commit**

```bash
git add experiments/configs/TagMApoca_ppo_sparse.yaml experiments/configs/TagMApoca_ppo_shaped.yaml experiments/configs/TagMApoca_ppo_smoke.yaml
git commit -m "feat: PPO trainer configs (sparse/shaped/smoke) for the 2x2 comparison"
```

*(The `config/ppo/*` copies live in the separate ml-agents repo, which is not this git repo — only the `experiments/configs/` archive is committed here, matching the existing convention.)*

---

## Task 3: Batch runner `run_ppo.bat`

**Files:**
- Create: `experiments/run_ppo.bat`

Mirrors `experiments/run_overnight_poca.bat` (same ENV path, `cd` to the ml-agents repo, per-run logs, `--no-graphics`), but only the two PPO runs.

- [ ] **Step 1: Create `experiments/run_ppo.bat`**

```bat
@echo off
setlocal
REM ============================================================================
REM  PPO COMPARISON — 2 runs (PPO sparse x1, PPO shaped x1), seed 1, 5M each.
REM  Completes the 2x2 (algorithm x reward) vs the 3-seed MA-POCA bands.
REM  PREREQUISITE: run the PPO SMOKE gate first (see docs plan Task 4) and confirm
REM  PPO trains + tolerates grouped agents BEFORE launching these 5M runs.
REM  Open the Anaconda Prompt (conda env "mlagents" active) and run this file.
REM ============================================================================

REM === full path to the built headless executable (same build as the POCA batch) ===
set "ENV=C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe"

REM ML-Agents repo root (configs are read relative to here)
cd /d C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
set "CFG=config\ppo"

if not exist "%ENV%" (
  echo [ERROR] Headless build not found at "%ENV%".
  pause
  exit /b 1
)
if not exist batch_logs mkdir batch_logs

echo Starting PPO batch at %DATE% %TIME%

mlagents-learn %CFG%\TagMApoca_ppo_sparse.yaml --env="%ENV%" --no-graphics --run-id=PPO_sparse_s1 --seed 1 > batch_logs\PPO_sparse_s1.log 2>&1
mlagents-learn %CFG%\TagMApoca_ppo_shaped.yaml --env="%ENV%" --no-graphics --run-id=PPO_shaped_s1 --seed 1 > batch_logs\PPO_shaped_s1.log 2>&1

echo PPO batch complete at %DATE% %TIME%
endlocal
pause
```

- [ ] **Step 2: Commit**

```bash
cd "c:/Users/david/Documents/PROGRAMMING/UnityProjects/TagMApoca_V1"
git add experiments/run_ppo.bat
git commit -m "feat: run_ppo.bat — unattended 2-run PPO batch for the 2x2 comparison"
```

---

## Task 4: PPO smoke-test gate (USER-run) — decision point

**No files.** This is the mandatory verification the spec requires *before* spending 5M compute. Claude prepares the command; the user runs it in the Anaconda Prompt and reports the console.

- [ ] **Step 1: (USER) Run the 50k PPO smoke**

In the Anaconda Prompt (conda `mlagents` active), with the headless build present:

```bash
cd C:\Users\david\Documents\PROGRAMMING\ML_AGENTS_GIT\ml-agents
mlagents-learn config/ppo/TagMApoca_ppo_smoke.yaml --env="C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\Build\TagMApoca_V1.exe" --no-graphics --run-id=PPO_smoke_01 --seed 1
```

- [ ] **Step 2: (USER + Claude) Check the three gate criteria**

Read `results/PPO_smoke_01/run_logs/` + the console summaries:
1. **PPO actually trains** — periodic `Step:` summaries advance; finite `Policy/Loss` and `Value Loss`, and **NO `Baseline Loss`** (its absence confirms PPO, since BaselineLoss is POCA-specific).
2. **No error/crash from grouped agents** — the run does not throw on agents registered in a `SimpleMultiAgentGroup`. A *warning* that group rewards are ignored is expected and fine; a hard error is not.
3. **Terminal signal reaches PPO** — `Environment/Cumulative Reward` (individual) moves off the pure −0.001×steps floor as occasional catches land (the individual ±1 is being received).

- [ ] **Step 3: Decision**

- **Clean (all three):** proceed to Task 5 with Approach 1 unchanged.
- **Broken** (errors on grouped agents, or no learning signal): apply the **Approach 2 fallback** — in `TagArenaManager`, when `IndividualTerminalRewardOn()` is true, replace the group episode-end calls with per-agent ends (`chaser.EndEpisode(); runner.EndEpisode();` for a catch; `EpisodeInterrupted()` for stalemate) instead of `EndGroupEpisode()`/`GroupEpisodeInterrupted()`, so the PPO arm uses no group objects. Re-run this smoke. Document the switch in Theory §13. If PPO + groups is intractable, dropping the PPO arm is the acceptable last resort (lowest-priority item per the spec).

---

## Task 5: Launch the two 5M PPO runs (USER-run)

**No files.** After the smoke gate passes.

- [ ] **Step 1: (USER) Run the batch**

In the Anaconda Prompt (conda `mlagents` active):

```bash
cd C:\Users\david\Documents\PROGRAMMING\UnityProjects\TagMApoca_V1\experiments
run_ppo.bat
```

Expected: silent terminal (output redirected to `batch_logs\PPO_*.log`), ~4–5 h/run (~1 overnight for both). Monitor via `type ...\ml-agents\batch_logs\PPO_sparse_s1.log` or TensorBoard (`tensorboard --logdir results`).

- [ ] **Step 2: (USER + Claude) Confirm completion**

Both `results/PPO_sparse_s1/` and `results/PPO_shaped_s1/` exist with `Chaser.onnx` + `Runner.onnx`. Copy the 4 brains into the project for inference/watching:

```bash
cd "c:/Users/david/Documents/PROGRAMMING/UnityProjects/TagMApoca_V1"
mkdir -p Assets/Models/ppo
SRC="C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents/results"
for r in sparse shaped; do cp "$SRC/PPO_${r}_s1/Chaser.onnx" "Assets/Models/ppo/${r}_s1_Chaser.onnx" && cp "$SRC/PPO_${r}_s1/Runner.onnx" "Assets/Models/ppo/${r}_s1_Runner.onnx"; done
git add Assets/Models/ppo
git commit -m "results: PPO 2x2 trained brains (sparse/shaped, 5M)"
```

---

## Task 6: 2×2 analysis + Theory §13 (after runs)

**Files:**
- Modify: `docs/Theory.md` (new §13)
- Create: `docs/figures/ppo/` (TensorBoard captures)

- [ ] **Step 1: Pull the final PPO numbers**

```bash
cd "C:/Users/david/Documents/PROGRAMMING/ML_AGENTS_GIT/ml-agents"
for f in PPO_sparse_s1 PPO_shaped_s1; do echo "== $f =="; grep -E "ELO" "batch_logs/$f.log" | tail -n 2; grep -E "Mean Group Reward" "batch_logs/$f.log" | tail -n 2; done
```

- [ ] **Step 2: Capture curves (TensorBoard → Playwright)**

Launch `tensorboard --logdir results` (Anaconda Prompt); use Playwright to screenshot ELO, Group Cumulative Reward, and Catch/Episode-Length for `PPO_*` alongside `POCA_*`; save to `docs/figures/ppo/`.

- [ ] **Step 3: Write Theory §13**

Add a `## 13. PPO comparison (2×2 algorithm × reward)` section: the 2×2 table (ELO gap, catch rate, episode length, Group Cumulative Reward for all four cells), the two figures, and the verdict against the two success criteria — (a) PPO_sparse ≈ POCA_sparse (emergent pursuit), (b) PPO_shaped ≈ POCA_shaped (farming trap) — supporting "MA-POCA ≈ PPO at 1v1, so the meaningful algorithmic comparison needs multi-agent groups."

- [ ] **Step 4: Commit**

```bash
cd "c:/Users/david/Documents/PROGRAMMING/UnityProjects/TagMApoca_V1"
git add docs/Theory.md docs/figures/ppo
git commit -m "docs: PPO 2x2 comparison results + figures (Theory §13)"
```

---

## Self-Review

**Spec coverage:** Component 1 (flag) → Task 1. Component 2 (configs) → Task 2. Component 3 (smoke gate + Approach-2 fallback) → Task 4. Component 4 (batch) → Task 3 + Task 5. Component 5 (analysis/§13) → Task 6. Verification (EditMode green, no-regression via default-off, smoke) → Task 1 Steps 4-5 + Task 4. All covered.

**Placeholder scan:** every code/config step shows full content; commands are exact. Task 2 Step 2/3 describe deltas from the fully-shown Step 1 config (word swap + `environment_parameters`/`max_steps` values) rather than repeating 74 lines — the changed lines are given explicitly, so no hidden content.

**Type/name consistency:** `IndividualTerminalRewardOn()` defined in Task 1 Step 1 and used in Steps 2-3; env-param key `individual_terminal_reward` matches across the helper and all three configs; `distance_shaping_coef` matches the existing TagAgent read; run-ids `PPO_sparse_s1`/`PPO_shaped_s1` consistent across Tasks 3/5/6; `Agent.AddReward` used on the manager's `chaser`/`runner` fields (public API).

**No-regression:** flag defaults to `0f` → `IndividualTerminalRewardOn()` false → both new branches skipped → POCA reward/termination path unchanged.
