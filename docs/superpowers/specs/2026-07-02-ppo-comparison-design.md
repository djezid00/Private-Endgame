# Design — PPO Comparison Arm (2×2 algorithm × reward)

> Amendment to `2026-06-20-1v1-rigor-phase-design.md`. That spec scoped a single PPO **shaped**
> sanity run. The 5M results (Theory.md §12) changed the picture — sparse = emergent chaser pursuit,
> shaped = proximity-farming (chaser loses) — so the PPO arm is re-scoped here to a **2×2**.
> Branch: `feat/ppo-comparison` (off `feat/sparse-vs-shaped-comparison`).

## Goal

Complete a **2×2 (algorithm × reward)** experiment by adding two PPO runs — **`PPO_sparse_s1`**
(`distance_shaping_coef = 0`) and **`PPO_shaped_s1`** (`distance_shaping_coef = 0.5`) — and comparing
them against the existing 3-seed MA-POCA bands.

|         | sparse (coef 0) | shaped (coef 0.5) |
|---------|-----------------|-------------------|
| MA-POCA | ✅ 3 seeds done | ✅ 3 seeds done |
| PPO     | **NEW ×1**      | **NEW ×1**        |

## Research purpose

At 1v1 each `SimpleMultiAgentGroup` has size 1, so MA-POCA's counterfactual group-credit reduces to a
single-agent return — functionally almost identical to PPO. The 2×2 tests two claims:

1. **Equivalence (sparse):** does PPO reach the *same* emergent pursuit MA-POCA got on the pure terminal
   reward (POCA_sparse chaser ELO ≈ 1890)? Expected **yes** → confirms the group machinery adds nothing
   at group-size-1, and that the genuine MA-POCA-vs-PPO comparison belongs in the later team-expansion
   phase.
2. **Trap is algorithm-independent (shaped):** does PPO *also* collapse into PBS proximity-farming
   (Group Reward ≈ −1, high shaping reward) like POCA_shaped? Expected **yes** → the farming pathology
   is a property of the reward, not the credit-assignment algorithm.

This is a **sanity/equivalence check**, not the headline result — hence 1 seed per PPO condition
(single lines against POCA's error bands), not 3.

## Scope decisions (locked in brainstorm)

- **Reward arms:** both sparse and shaped (full 2×2).
- **Seeds:** **1 per PPO condition** (2 runs total), `--seed 1` so each lines up with `POCA_*_s1`.
- **Budget:** `max_steps: 5000000` to match POCA for a fair comparison; early-stop on plateau allowed.
- **Everything else identical** to the POCA configs (observations, network, self-play) so the only
  variables are algorithm and reward arm.

## Component 1 — `individual_terminal_reward` flag (guarded mirror) — Approach 1

PPO ignores group rewards (`AddGroupReward`/`EndGroupEpisode` are POCA-only), so without a change the
PPO runs would train on only the −0.001/step term and never learn about catching. Fix:

- `TagArenaManager` reads `Academy.Instance.EnvironmentParameters.GetWithDefault(
  "individual_terminal_reward", 0f)` (read once, mirroring how `distance_shaping_coef` is handled).
- When the flag is **on** (> 0.5), `OnAgentTagged` and `TriggerStalemate` **additionally** call
  `AddReward` on the chaser and runner agents with the **exact same values delivered to the groups**:
  - **Catch:** chaser `+1 + timeBonus`, runner `−1 + survivalBonus`
    (`timeBonus = clamp(1 − steps/maxSteps, 0, 0.5)`, `survivalBonus = clamp(steps/maxSteps, 0, 0.5)`).
  - **Stalemate:** runner `+1`, chaser `−1`.
- Episodes still end **through the group** exactly as today (`EndGroupEpisode` / `GroupEpisodeInterrupted`).
  At group-size-1, individual == group, so the comparison is provably fair.
- Flag **defaults to 0** ⇒ the POCA reward/termination path is **byte-identical** — no regression.

**Why not switch to per-agent `EndEpisode` (Approach 2):** it would make the two algorithms run
*different* episode-termination code — a confound. Only adopt it if the smoke test (Component 3) shows
PPO genuinely cannot tolerate grouped agents; document the switch if so.

## Component 2 — PPO trainer config `TagMApoca_ppo.yaml`

- Location: ml-agents `config/ppo/TagMApoca_ppo.yaml` (+ archived copy in `experiments/configs/`).
- `trainer_type: ppo`; **identical** `network_settings` (normalize, hidden_units 256, num_layers 2) and
  `self_play` block as the POCA configs; standard PPO `hyperparameters`; `max_steps: 5000000`;
  `time_horizon`, `summary_freq`, `checkpoint_interval`, `keep_checkpoints` matched to the 5M POCA runs.
- Behavior blocks: `Chaser:` and `Runner:` (same behavior names/prefabs as POCA).
- Two runs differ **only** by one `environment_parameters` value:
  - `PPO_sparse_s1`: `{ distance_shaping_coef: 0.0, individual_terminal_reward: 1.0 }`
  - `PPO_shaped_s1`: `{ distance_shaping_coef: 0.5, individual_terminal_reward: 1.0 }`

## Component 3 — Smoke-test gate (MANDATORY before the 5M runs)

Run a short (~50k-step) PPO run against the headless build and verify from the console + a checkpoint:

1. Both behaviors connect and **PPO actually trains** (finite PolicyLoss/ValueLoss; **no** BaselineLoss,
   which is POCA-specific — its *absence* confirms PPO, mirroring §2's presence-confirms-POCA logic).
2. ML-Agents does **not** error or mis-behave on agents still registered in a `SimpleMultiAgentGroup`.
3. The individual terminal reward is being received (Group Cumulative Reward moves off a pure-stalemate
   −1 as occasional catches land, same as POCA early training).

**Decision point:** clean → proceed with Approach 1 for the full runs. Broken (errors / no learning
signal) → fall back to Approach 2 (per-agent `EndEpisode`, skip group registration for the PPO arm),
document it, then re-smoke. If PPO + grouped agents proves intractable, dropping the PPO arm is an
acceptable last resort (it is the lowest-priority item, per the parent spec).

## Component 4 — Run + batch

- Two 5M runs against the **headless build** (`Build/TagMApoca_V1.exe`), **16 arenas**, `--no-graphics`,
  per-run logs, mirroring `experiments/run_overnight_poca.bat`.
- New `experiments/run_ppo.bat` with the two `mlagents-learn` commands (run-ids `PPO_sparse_s1`,
  `PPO_shaped_s1`, both `--seed 1`), sequential/unattended, continue-on-failure, tee to `batch_logs/`.
- (Conda only activates in the Anaconda Prompt; Claude prepares the commands, the user runs them.)

## Component 5 — Analysis & write-up

- Overlay the two PPO lines on the POCA 3-seed error bands (reuse the seed-aggregation figures).
- A **2×2 summary table**: ELO (chaser−runner gap), catch rate, episode length, Group Cumulative Reward.
- Capture curves (TensorBoard → Playwright) into `docs/figures/ppo/`; write results into a new
  `Theory.md` **§13**.

## Verification

- EditMode tests (`TagRewardTests`) remain green (pure shaping math untouched).
- **No-regression proof:** a POCA smoke with the flag **off** produces the same reward/termination
  behavior as before (flag defaults to 0).
- Smoke test (Component 3) proves PPO trains on the individual reward before any 5M compute is spent.

## Success criteria

- **Equivalence:** PPO_sparse ≈ POCA_sparse (chaser reaches emergent pursuit; ELO strongly chaser-favored).
- **Trap:** PPO_shaped ≈ POCA_shaped (chaser farms proximity, Group Reward ≈ −1).
- Both support the thesis argument that MA-POCA ≈ PPO at 1v1, so the meaningful algorithmic comparison
  requires multi-agent groups (team-expansion phase).

## Out of scope (deferred)

- 3-seed PPO error bands (this is a sanity arm; revisit only if the 2×2 looks surprising).
- Team expansion, grabbable cubes, obstacles, heuristic-bot baseline (parent spec's later phases).
- The reset-ordering cleanup (chaser `OnEpisodeBegin` drives arena reset) — still a latent follow-up.
