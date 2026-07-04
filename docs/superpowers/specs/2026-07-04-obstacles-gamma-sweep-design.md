# Design — Obstacles × Gamma Sweep (+ shaped-trap gamma probe)

> Next experimental phase after the PPO 2×2 (Theory.md §13). Combines the two ideas from the
> 2026-07-04 brainstorm — arena obstacles and hyperparameter influence — into one factorial phase
> whose baseline cells are the existing 5M runs. Branch: `feat/obstacles-gamma-sweep`
> (off `feat/ppo-comparison`).

## Goal

Measure how the **discount factor γ** (the agent's effective planning horizon) shapes emergent
pursuit-evasion in an arena **with obstacles**, and use low-γ **shaped probes** to test the §12
farming-trap mechanism directly. ~20 runs of 5M steps/behavior total.

## Research questions

- **RQ-A (gamma sensitivity):** how does γ ∈ {0.8, 0.9, 0.95, 0.99, 0.995} affect emergent
  pursuit-evasion (catch rate, ELO gap, episode length) under MA-POCA self-play in an obstacle
  arena? Output: a 5-point sensitivity curve — the "why is gamma always 0.99?" answer.
- **RQ-B (trap mechanism probe):** does lowering γ modulate the PBS proximity-farming trap
  (§12)? Tests the pre-registered mechanism (the γ-dependent invariance-violating term).
- **RQ-C (obstacles):** does line-of-sight-breaking cover produce new emergent behavior
  (interception around cover, cover use by the runner), and does a randomized layout change what
  is learned vs a fixed layout?

## Pre-registered expectations (write into Theory.md §14 BEFORE any batch launches)

**New standing workflow rule:** from this phase on, each Theory.md results section opens with a
*Pre-registered expectations* subsection written before the runs; findings are then reported
against those predictions.

- **RQ-B (probe) — mechanism math.** For a stationary agent the per-step shaping reward is
  `F = γΦ − Φ = (1−γ)·coef·(d/maxDist) ≥ 0` — the invariance-violating "standing reward" scales
  with **(1−γ)**. At γ=0.8 it is **20×** the γ=0.99 case; simultaneously the future terminal +1
  is discounted harder. Both effects point the same way. **Prediction: farming worsens at lower γ
  (catch rate ≤ the ~0.01 γ=0.99 baseline, Group Reward pinned at ≈ −1).** A material catch-rate
  *rise* at low γ would falsify the mechanism story. Note the term grows with *distance*, so at
  γ=0.8 the chaser may even learn to keep distance rather than hover — watch for this signature
  (mean chaser–runner distance), and reconcile the write-up with §11's "standing reward for being
  close" phrasing, which this derivation refines.
- **RQ-A (sweep).** Prediction: catch rate and ELO gap **increase with γ up to ~0.99, then
  plateau or dip slightly at 0.995** (an inverted-U / saturating curve). Rationale: γ=0.8 gives an
  effective horizon of ~5 decisions (~25 physics steps) — too myopic to plan interception around
  cover; 0.995 extends the horizon past half the episode, adding credit-assignment noise with
  little planning benefit. Falsified if the curve is flat (γ doesn't matter here) or monotonic
  in the opposite direction.
- **RQ-C (obstacles).** Prediction: with 4 fixed pillars the sparse γ=0.99 chaser still clearly
  beats the runner but below the open-arena ceiling (catch rate well above the ~0.1 random
  baseline, below ~1.0); randomized layouts learn slower and end lower than fixed at matched γ.
  Qualitatively: runner uses pillars to break line of sight; chaser learns cut-off routes
  (fixed layout) vs general navigation (random).

## Experiment matrix (~20 runs × 5M steps/behavior, 16 arenas, headless `--no-graphics`)

| Set | Environment | Reward arm | γ values | Seeds | Runs |
|---|---|---|---|---|---|
| **Probes** | no obstacles | shaped (coef 0.5) | 0.8, 0.9 | 1 | 2 |
| **Phase A** | 4 fixed pillars | sparse (coef 0) | 0.8, 0.9, 0.95, 0.99, 0.995 | 3 at γ∈{0.8, 0.995}, else 1 | 9 |
| **Phase B** | 4 pillars, randomized per episode | sparse (coef 0) | same | same | 9 |

- **Anchor cells (already run, no new compute):** `POCA_shaped_s{1,2,3}` = probe γ=0.99 baseline;
  `POCA_sparse_s{1,2,3}` = open-arena γ=0.99 sparse baseline.
- γ is set **symmetrically in both the `Chaser` and `Runner` blocks** (else the sweep confounds
  role asymmetry with discounting). All other hyperparameters identical to the 5M rigor configs.
- Run-id scheme: `POCA_shaped_g080_s1`; `POCA_sparse_obsF_g099_s1` (fixed) /
  `POCA_sparse_obsR_g099_s1` (random). Seeds 1–3 as in the rigor phase.

## Scope decisions (locked in brainstorm)

- **Sequencing: Approach B (pipelined).** Probes need only the `shaping_gamma` env-param →
  implement + smoke + launch overnight **while the obstacle system is built**; then Phase A;
  Phase A → Phase B is a **decision gate**, not an automatic step.
- **Obstacles are hand-authored by the user in Unity** (Phase A layout is the user's); code is
  limited to activation + Phase-B randomization of those same objects.
- **Obstacles reuse the `"Wall"` tag** → `RayPerceptionSensor3D` already sees them, observation
  space unchanged, all prior runs stay directly comparable; `num_obstacles = 0` reproduces the
  legacy arena exactly.
- **Seeds:** 3 only at the sweep endpoints (variance bars where the claim is boldest), 1
  elsewhere, 1 for probes — same honest-caveat pattern as the PPO arm.
- **5M steps/behavior** to match all prior runs.

## Component 1 — `shaping_gamma` env-param (probe prerequisite)

- `TagAgent` currently hard-codes `shapingGamma = 0.99f` (inspector field,
  `Assets/Scripts/TagAgent.cs:23`, comment "MUST match trainer extrinsic.gamma").
- Change: read `Academy.Instance.EnvironmentParameters.GetWithDefault("shaping_gamma",
  shapingGamma)` once per episode, exactly like `distance_shaping_coef`. **Default = the
  inspector value ⇒ every existing config byte-identical.**
- Every new YAML sets `shaping_gamma` equal to the trainer's `extrinsic.gamma` (no-op when
  `distance_shaping_coef = 0`, but kept consistent everywhere).

## Component 2 — Obstacle system

- **User authors** 4 pillar GameObjects under an `Obstacles` parent in the `TagArena` prefab:
  tag `"Wall"`, static BoxColliders, ~2 u tall (raycasts at agent height must hit them),
  mirror-symmetric (fair for both roles), ≥1.5 u clear of walls. Claude reviews the prefab
  (tags/colliders/height/symmetry) before the smoke gate.
- **`ObstacleManager`** (new, on the TagArena prefab) reads two env-params per episode:
  - `num_obstacles` (default 0): activates the first N authored pillars; 0 = legacy arena.
  - `obstacle_layout` (default 0): `0` = leave pillars where the user authored them (fixed);
    `1` = reposition all active pillars each episode — random position + Y-rotation via
    rejection sampling with clearance rules (inside bounds; min separation from each other,
    the walls, and both agent spawn points).
- **`ObstaclePlacement`** — pure placement math (bounds, clearance, rejection sampling with
  bounded attempts + deterministic fallback) in the unit-tested asmdef, same pattern as
  `TagReward`. EditMode tests cover bounds, clearance and separation invariants.
- **Agent spawning:** `TagArenaManager` spawn keeps its current bands but re-samples if the
  candidate position is within clearance of an active obstacle (bounded retries).
- Reset order: obstacles reposition first, then agents spawn (inside the existing arena reset).

## Component 3 — Trainer configs

- 12 new YAMLs in ml-agents `config/poca/` + archived in `experiments/configs/`:
  - Probes: `TagMApoca_shaped_g080.yaml`, `TagMApoca_shaped_g090.yaml`
    (= `TagMApoca_shaped_5M.yaml` except `gamma` ×2 blocks + `shaping_gamma`).
  - Phase A: `TagMApoca_sparse_obsF_g{080,090,095,099,0995}.yaml`
    (`num_obstacles: 4`, `obstacle_layout: 0`).
  - Phase B: `TagMApoca_sparse_obsR_g{080,090,095,099,0995}.yaml` (`obstacle_layout: 1`).
- Each config's diff vs the 5M rigor config = **only** gamma (both blocks), `shaping_gamma`,
  and the obstacle env-params. Everything else untouched.

## Component 4 — Batch scripts

Same pattern as `run_overnight_poca.bat` (headless build, per-run logs to `batch_logs/`,
sequential, continue-on-failure):

- `experiments/run_gamma_probes.bat` — 2 runs (~8 h, overnight #1).
- `experiments/run_obs_phaseA.bat` — 9 runs (~36–42 h).
- `experiments/run_obs_phaseB.bat` — 9 runs (~36–42 h).

(Conda only activates in the Anaconda Prompt; Claude prepares commands, the user launches them.)

## Component 5 — Gates (in order)

1. **EditMode tests green** (existing 5 + new `ObstaclePlacement` tests) + Editor verify
   (WASD near pillars, clean resets, no Console errors, rays hit pillars).
2. **Rebuild headless binary after EVERY code change** before training against it (PPO-phase
   lesson: the binary must contain the new env-param code).
3. **50k smoke gate per new binary** before any 5M batch: probes smoke checks `shaping_gamma`
   is read (log line / behavior); obstacle smoke checks pillars activate, reposition (Phase-B
   mode), no cross-arena bleed, catches still register, `TimeToCatch` nonzero.
4. **Phase A → Phase B decision gate:** review Phase A results (against the pre-registered
   expectations) before spending Phase B's ~36 h.

## Component 6 — Analysis & write-up (Theory.md §14)

- **Pre-registered expectations subsection first** (see above), committed before launch.
- Sensitivity figures: catch rate vs γ and ELO gap vs γ (5 points, error bars at the 3-seed
  endpoints), one curve per obstacle phase; open-arena γ=0.99 anchor marked.
- Probe figure: shaped catch rate for γ ∈ {0.8, 0.9, 0.99 (3-seed baseline)} — extends §13's
  Fig 7 delivery-probe style; plus mean chaser–runner distance if the distance-keeping
  signature appears.
- Fixed-vs-random comparison at matched γ; qualitative Editor-inference behavior notes
  (cover use, interception routes).
- Reuse the existing tfevents parser + seed-aggregation + figure pipeline
  (`docs/figures/gamma/`).

## Verification

- Flag-off no-regression: with no new env-params set, reward math and arena behavior are
  byte-identical (defaults preserve current values); existing EditMode tests stay green.
- Probe smoke proves `shaping_gamma` reaches the PBS term before probe compute is spent.
- Obstacle smoke proves the environment is mechanically sound before Phase A compute.

## Success criteria & fallbacks

- **Probes:** either direction is a citable result (worse farming confirms the mechanism; a
  rescue falsifies it) — success = clean attribution, since gamma is the only changed variable.
- **Phase A:** success = a resolvable trend across γ (differences beyond the endpoint seed
  bands). **Pre-registered fallback:** if the γ=0.99 fixed-obstacle chaser is still at the
  random baseline at 5M (obstacles too hard at equal speed), rerun Phase A with **2 pillars**
  (documented change), mirroring the §9 fallback pattern.
- **Phase B:** success = fixed-vs-random contrast at matched γ is interpretable (generalization
  cost quantified).

## Out of scope (deferred)

- Other hyperparameters (λ, time_horizon, buffer sizes…) — gamma only; the method generalizes.
- Team expansion (2v1/2v2 — where the real MA-POCA-vs-PPO comparison lives), grabbable cubes,
  moving obstacles, curriculum on obstacle count.
- More PPO seeds / extending `POCA_shaped_indivterm` (§13 optional hardening).
- The reset-ordering cleanup (chaser `OnEpisodeBegin` drives arena reset) — still a latent
  follow-up.
