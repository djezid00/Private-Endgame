# Design — Chaser Reward Shaping as a Sparse-vs-Shaped Experiment

**Date:** 2026-06-17
**Branch:** `feat/ma-poca-asymmetric-refactor`
**Status:** approved design, pending implementation plan
**Related:** `docs/Theory.md` §6 (risk + levers), `docs/progress.md`

## Purpose

The smoke run (`TagTest_poca_01`) showed the random-policy baseline is ~100% runner-win-by-stalemate
with a sparse, ~5–15% accidental catch rate, and the chaser's only guidance (uniform −0.001/step) gives
no spatial gradient toward the runner. With **equal kinematics** (both `moveSpeed 5`) this is the
project's central risk: the chaser may learn very slowly or stall.

Rather than guess a reward, we make the question itself the experiment: **does MA-POCA + self-play
learn pursuit from the pure terminal signal, and does dense distance shaping help?** The next training
effort is therefore **two run-arms** — *sparse* and *shaped* — identical except for one variable.

## Decisions (from brainstorming)

- **Scope:** reward + kinematics design for the next run only (full thesis experiment plan is a later
  brainstorm).
- **Shaping is the experiment:** report sparse vs shaped as a finding, not a hidden implementation choice.
- **Kinematics:** fixed **equal 5/5** across both arms; a **6/5 chaser edge** is a documented fallback
  if both arms fail the success bar (the fallback is itself a reportable result).
- **Shaped-reward form:** **potential-based shaping (PBS)**, Ng, Harada & Russell (1999) — chosen because
  it is provably policy-invariant, so it changes *learning speed, not the optimal policy*. This preserves
  the "emergence" claim and answers the "did you engineer the behavior?" critique.
- **Success rule (strict):** an arm is "healthy" only if **catch rate ↑ AND mean episode length ↓ AND
  ELO diverges** over the run. Both arms flat near baseline ⇒ trigger the 6/5 fallback.

## Experiment structure

Two arms, identical except the distance-shaping term:

| Component | Sparse arm | Shaped arm |
|---|---|---|
| Terminal reward (catch / stalemate) | ±1 + time/survival bonus | same |
| Chaser ±0.001/step time pressure | kept | kept |
| Chaser distance shaping (PBS) | **off** (`coef = 0`) | **on** (`coef = 0.5`) |
| Kinematics (chaser/runner moveSpeed) | 5 / 5 | 5 / 5 |
| Observations, network, self-play, seed | same | same |

The only difference is the distance term ⇒ a clean single-variable comparison. Runner reward is
unchanged in both arms.

## Reward specification (PBS)

- **Potential:** `Φ(s) = −coef · (planarDist(chaser, runner) / maxDist)`
  - `planarDist` = XZ-plane distance between the two agents.
  - `maxDist` = arena diagonal (~28 for the 20×20 floor); a fixed constant so `Φ ∈ [−coef, 0]`.
  - Closer ⇒ higher (less negative) potential.
- **Shaping reward** (chaser only, each decision step): `F = γ·Φ(s′) − Φ(s)`, with `γ = 0.99`
  (matches the trainer's `extrinsic.gamma`). Applied via `AddReward(F)`.
- **Bookkeeping:** store `prevPotential`, seeded in `OnEpisodeBegin` from the spawn positions (no shaping
  reward is emitted before the first transition).
- **Coefficient:** start `coef = 0.5` so `|Φ| ≤ 0.5` and the telescoped shaping contribution stays below
  the ±1 terminal signal. Because PBS is policy-invariant, `coef` affects only learning speed/stability,
  not the optimum, so it is safe to tune without invalidating the comparison.
- **Runner reward:** unchanged (terminal ±1 + survival bonus + ±0.001/step) in both arms.

## Implementation surface

- **`Assets/Scripts/TagAgent.cs`**
  - In `OnEpisodeBegin`: read the coefficient via
    `Academy.Instance.EnvironmentParameters.GetWithDefault("distance_shaping_coef", 0f)` and store it;
    seed `prevPotential` from the initial state.
  - Add a `Potential()` helper and apply the PBS term in `OnActionReceived` (chaser branch only).
  - Reading the coefficient per-episode (not per-step) ensures it cannot change mid-episode.
- **`Assets/Scripts/TagArenaManager.cs`**
  - Add a `StatsRecorder` and log per episode: **`catch`** (1 on a catch, 0 on stalemate) and
    **`time_to_catch`** (steps at catch). These surface as TensorBoard scalars → direct catch-rate and
    time-to-catch curves for the thesis.
- **Configs** (derived from the real `TagMApoca.yaml`, in the ML-Agents repo under `config/poca/`):
  - `TagMApoca_sparse.yaml` — `environment_parameters: { distance_shaping_coef: 0.0 }`
  - `TagMApoca_shaped.yaml` — `environment_parameters: { distance_shaping_coef: 0.5 }`
  - Both: `max_steps ≈ 400000`, `summary_freq ≈ 10000`, otherwise identical to the production config.
  - The arm is selected by **config**, never by a hand-toggled Editor field (reproducibility).

## Run protocol

1. Implement the code + config changes above.
2. Run both arms with the **same `--seed`** for fairness (~45 min/arm at 4 arenas on CPU).
   - Conda is only available in the Anaconda Prompt; Claude prepares the commands, user runs them and
     presses Play (see CLAUDE.md).
3. Capture curves via TensorBoard → Playwright (catch rate, episode length, ELO, reward).
4. Apply the strict success rule. If **both** arms stay flat near the random baseline
   (catch ~15%, length ~393 decision steps, ELO ~tied) → re-run with the **6/5 chaser edge** fallback.

## Metrics

- `Self-play/ELO` (divergence), `Environment/CumulativeReward` + `GroupCumulativeReward` (opposing curves),
  `Environment/EpisodeLength` (time-to-catch), and the new custom `catch` / `time_to_catch` stats,
  plus `Policy/Entropy` and the three loss terms (incl. POCA `BaselineLoss`).

## Risks & mitigations

- **Equal-speed stall (even shaped):** covered by the 6/5 fallback; the stall itself is a finding.
- **`coef` too high → instability:** reduce it; policy-invariance means lowering it does not harm the
  emergence claim.
- **PBS half-step timing:** `rb.MovePosition` applies on the next physics frame, so `Φ(s′)` read in the
  same `OnActionReceived` is a half-step stale; negligible for telescoped PBS. Noted, not corrected.
- **Self-play noise at 400k:** judge **trends**, not absolute values; same seed across arms reduces
  cross-arm variance.

## Out of scope (later brainstorms)

- The full thesis experiment matrix (seeds × ablations), arena-count scaling for the 5M run, and the
  PPO-vs-MA-POCA comparison.
- The pending reset-ordering cleanup (chaser `OnEpisodeBegin` drives the arena reset).
